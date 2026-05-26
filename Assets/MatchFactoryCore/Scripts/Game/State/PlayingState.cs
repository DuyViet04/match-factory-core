using System.Collections.Generic;
using DG.Tweening;
using MatchFactoryCore.Scripts.Item;
using MatchFactoryCore.Scripts.State;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class PlayingState : BaseState
    {
        private Camera _mainCamera;
        private const float DragThreshold = 15f;
        readonly List<GameObject> _itemSlots; // List UI là slot của Sprite
        readonly List<int> _dataTypes;
        readonly List<IItemFactory2D> _allBarSprites;
        Vector2 _startMousePos;
        bool _isPressing;
        bool _hasMovedEnoughForDrag;
        int _lastDragId = -1;
        GameObject _lastItemGameObject;
        List<Vector3> _cachePositionItemSlots;

        public PlayingState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(
            controller, stateMachine)
        {
            _itemSlots = Controller.ItemSlots;
            _dataTypes = new List<int>();
            _allBarSprites = new List<IItemFactory2D>();
        }

        public override void OnEnter()
        {
            Debug.Log("Enter Playing State");
            _mainCamera = Camera.main;
            Controller.ActiveInput(true);

            _cachePositionItemSlots = new List<Vector3>();
            foreach (var slot in _itemSlots)
            {
                var rectTransform = slot.GetComponent<RectTransform>();
                _cachePositionItemSlots.Add(rectTransform.anchoredPosition);
            }
        }

        public override void OnUpdate()
        {
            Controller.UpdateTimeLevel();

            if (Controller.TimeLevel <= 0)
            {
                StateMachine.ChangeState(MatchFactoryState.Lose);
                return;
            }

            // Input
            if (Pointer.current.press.wasPressedThisFrame)
            {
                var pos = Pointer.current.position.ReadValue();
                _startMousePos = pos;
                _isPressing = true;
                _hasMovedEnoughForDrag = false;
                _lastDragId = -1;
                _lastItemGameObject = null;

                var ray = _mainCamera.ScreenPointToRay(pos);
                Controller.Input.HandleClick(ray, out var go);
                if (go == null) return;

                var outline = go.GetComponentInChildren<Outline>();
                if (outline != null)
                {
                    outline.enabled = true;
                }

                IItemFactory3D itemFactory3D = go.GetComponent<IItemFactory3D>();
                itemFactory3D.RigidbodyObject.AddForce(Vector3.up, ForceMode.Impulse);
                _lastDragId = go.GetComponent<ItemFactory>().Id;
                _lastItemGameObject = go;
            }

            if (_isPressing)
            {
                var pos = Pointer.current.position.ReadValue();
                if (!_hasMovedEnoughForDrag)
                {
                    if (Vector2.Distance(_startMousePos, pos) > DragThreshold)
                    {
                        _hasMovedEnoughForDrag = true;
                    }
                }

                if (_hasMovedEnoughForDrag)
                {
                    var ray = _mainCamera.ScreenPointToRay(pos);
                    Controller.Input.HandleClick(ray, out var go);
                    if (go != null)
                    {
                        ItemFactory itemFactory = go.GetComponent<ItemFactory>();
                        IItemFactory3D itemFactory3D = itemFactory as IItemFactory3D;
                        if (itemFactory.Id != _lastDragId)
                        {
                            itemFactory3D.RigidbodyObject.AddForce(Vector3.up, ForceMode.Impulse);
                            _lastDragId = itemFactory.Id;

                            var outline = go.GetComponentInChildren<Outline>();
                            if (outline != null)
                            {
                                outline.enabled = true;
                            }

                            if (_lastItemGameObject != null)
                            {
                                var lastOutline = _lastItemGameObject.GetComponentInChildren<Outline>();
                                if (lastOutline != null)
                                {
                                    lastOutline.enabled = false;
                                }

                                _lastItemGameObject = go;
                            }
                        }
                    }
                }
            }

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                _isPressing = false;

                if (_lastItemGameObject != null)
                {
                    var outline = _lastItemGameObject.GetComponentInChildren<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }

                if (!_hasMovedEnoughForDrag)
                {
                    var pos = Pointer.current.position.ReadValue();
                    var ray = _mainCamera.ScreenPointToRay(pos);
                    Controller.Input.HandleClick(ray, out var go);
                    if (go != null)
                    {
                        ItemFactory itemFactory = go.GetComponent<ItemFactory>();
                        IItemFactory3D itemFactory3D = itemFactory as IItemFactory3D;
                        IItemFactory2D itemFactory2D = itemFactory as IItemFactory2D;

                        if (itemFactory != null && itemFactory3D != null && itemFactory2D != null)
                        {
                            CheckLevelTarget(go);

                            if (_dataTypes.Count >= MatchFactoryController.MaxSlot) return;
                            HandleObject(itemFactory3D, itemFactory2D);
                        }
                    }
                }
            }
        }

        private void HandleObject(IItemFactory3D itemFactory3D, IItemFactory2D itemFactory2D)
        {
            var itemComp = (ItemFactory)itemFactory2D;

            // 1. Insert vào _data2Ds, lấy index chính xác
            InsertData(itemFactory2D, (int)itemComp.FactoryType, out int newIndex);
            _allBarSprites.Insert(newIndex, itemFactory2D);

            // 4. CheckMatch — xóa khỏi data ngay lập tức
            CheckMatch((int)itemComp.FactoryType, out var matchs, out var isMatch, out var matchIndices);

            // 3. Tính items bị dịch, cập nhật CurrentIndex ngay
            var itemsToJump = new List<(IItemFactory2D item, int numJump)>();
            for (int i = 0; i < _allBarSprites.Count; i++)
            {
                var item2D = _allBarSprites[i];
                if (item2D == null) continue;
                var comp = (ItemFactory)item2D;
                if (comp.CurrentIndex != i)
                {
                    int numJump = Mathf.Abs(comp.CurrentIndex - i);
                    itemsToJump.Add((item2D, numJump));
                    comp.CurrentIndex = i;
                }
            }

            itemFactory3D.JumpFromBoard(
                itemFactory3D.Prefab.transform.position + new Vector3(0, 5, -1),
                () =>
                {
                    itemFactory3D.ChangeTo2D(() =>
                    {
                        itemFactory2D.MoveToBar(_itemSlots, _mainCamera, newIndex, () =>
                        {
                            if (isMatch)
                            {
                                itemFactory2D.Match(_itemSlots, _mainCamera, matchs, () =>
                                {
                                    for (int i = matchIndices.Count - 1; i >= 0; i--)
                                    {
                                        // _allBarSprites.RemoveAt(matchIndices[i]);
                                    }
                                });
                            }

                            CheckWinLose(isMatch);
                            _itemSlots[newIndex].GetComponent<RectTransform>()
                                .DOPunchAnchorPos(Vector2.down * 20f, 0.5f, 3, 5);
                        });

                        foreach (var (item, numJump) in itemsToJump)
                        {
                            var capturedItem = item;
                            capturedItem.JumpOnBar(_itemSlots, _mainCamera, numJump, () =>
                            {
                                var comp = (ItemFactory)capturedItem;
                                var slotRect = _itemSlots[comp.CurrentIndex].GetComponent<RectTransform>();
                                slotRect?.DOPunchAnchorPos(Vector2.down * 20f, 0.25f, 3, 5)
                                    .OnComplete(() =>
                                    {
                                        slotRect.anchoredPosition = _cachePositionItemSlots[comp.CurrentIndex];
                                    });
                            });
                        }
                    });
                });
        }

        private void CheckMatch(int type, out List<IItemFactory2D> matchs, out bool isMatch, out List<int> matchIndices)
        {
            matchs = new List<IItemFactory2D>();
            matchIndices = new List<int>();
            isMatch = false;

            for (int i = 0; i < _dataTypes.Count; i++)
            {
                if (_dataTypes[i] == type)
                {
                    matchs.Add(_allBarSprites[i]);
                    matchIndices.Add(i);
                }

                if (matchs.Count == 3)
                {
                    isMatch = true;
                    break;
                }
            }

            if (isMatch)
            {
                for (int i = matchIndices.Count - 1; i >= 0; i--)
                {
                    _dataTypes.RemoveAt(matchIndices[i]);
                    _allBarSprites.RemoveAt(matchIndices[i]);
                }
            }
        }

        private void CheckWinLose(bool isMatch)
        {
            if (IsWin())
            {
                StateMachine.ChangeState(MatchFactoryState.Win);
            }
            else if (IsLose(isMatch))
            {
                StateMachine.ChangeState(MatchFactoryState.Lose);
            }
        }

        void CheckLevelTarget(GameObject go)
        {
            var itemFactory = go.GetComponent<ItemFactory>();
            if (itemFactory == null) return;

            var type = itemFactory.FactoryType;
            if (Controller.TargetDictionary.TryGetValue(type, out var currentTarget))
            {
                if (currentTarget > 0)
                {
                    Controller.ClickTarget(Controller.TargetDictionary[type]--);
                }
            }
        }

        bool IsWin()
        {
            bool isWin = true;
            foreach (var item in Controller.TargetDictionary)
            {
                if (item.Value != 0)
                {
                    isWin = false;
                    break;
                }
            }

            return isWin;
        }

        bool IsLose(bool isMatch)
        {
            bool isLose = false;
            if (Controller.TimeLevel <= 0)
            {
                isLose = true;
            }
            else if (_allBarSprites.Count == MatchFactoryController.MaxSlot && !isMatch)
            {
                isLose = true;
            }

            return isLose;
        }

        private void InsertData(IItemFactory2D itemFactory2D, int type, out int index)
        {
            index = -1;
            int maxSlot = MatchFactoryController.MaxSlot;

            if (_dataTypes.Count == 0)
            {
                _dataTypes.Add(type);
                index = 0;
                ((ItemFactory)itemFactory2D).CurrentIndex = index;
                return;
            }

            bool isInsert = false;
            for (int i = _dataTypes.Count - 1; i >= 0; i--)
            {
                if (_dataTypes.Count >= maxSlot) break;
                if (_dataTypes[i] == type)
                {
                    _dataTypes.Insert(i + 1, type);
                    index = i + 1;
                    isInsert = true;
                    break;
                }
            }

            if (!isInsert)
            {
                _dataTypes.Add(type);
                index = _dataTypes.Count - 1;
            }

            ((ItemFactory)itemFactory2D).CurrentIndex = index;
        }
    }
}
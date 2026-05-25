using System.Collections.Generic;
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
        readonly List<int> _data2Ds;
        readonly List<IItemFactory2D> _allBarSprites;
        bool _isMatch;
        Vector2 _startMousePos;
        bool _isPressing;
        bool _hasMovedEnoughForDrag;
        int _lastDragId = -1;

        public PlayingState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(
            controller, stateMachine)
        {
            _itemSlots = Controller.ItemSlots;
            _data2Ds = new List<int>();
            _allBarSprites = new List<IItemFactory2D>();
        }

        public override void OnEnter()
        {
            Debug.Log("Enter Playing State");
            _mainCamera = Camera.main;
            Controller.ActiveInput(true);
        }

        public override void OnUpdate()
        {
            Controller.UpdateTimeLevel();

            if (Controller.TimeLevel <= 0)
            {
                StateMachine.ChangeState(MatchFactoryState.Lose);
                return;
            }

            if (Pointer.current.press.wasPressedThisFrame)
            {
                var pos = Pointer.current.position.ReadValue();
                _startMousePos = pos;
                _isPressing = true;
                _hasMovedEnoughForDrag = false;
                _lastDragId = -1;

                var ray = _mainCamera.ScreenPointToRay(pos);
                Controller.Input.HandleClick(ray, out var go);
                if (go == null) return;

                IItemFactory3D itemFactory3D = go.GetComponent<IItemFactory3D>();
                itemFactory3D.RigidbodyObject.AddForce(Vector3.up, ForceMode.Impulse);
                _lastDragId = go.GetComponent<ItemFactory>().Id;
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
                        }
                    }
                }
            }

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                _isPressing = false;

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
                            HandleObject3D(itemFactory3D);
                            CheckLevelTarget(go);
                            HandleObject2D(itemFactory2D);

                            // Win/Lose
                            if (IsWin())
                            {
                                StateMachine.ChangeState(MatchFactoryState.Win);
                            }

                            if (IsLose(_isMatch))
                            {
                                StateMachine.ChangeState(MatchFactoryState.Lose);
                            }
                        }
                    }
                }
            }
        }

        private void HandleObject3D(IItemFactory3D itemFactory3D)
        {
            itemFactory3D.JumpFromBoard(itemFactory3D.Prefab.transform.position + new Vector3(0, 5, -1));
            itemFactory3D.ChangeTo2D();
        }

        private void HandleObject2D(IItemFactory2D itemFactory2D)
        {
            // Move
            itemFactory2D.MoveToBar(_itemSlots, _mainCamera, _data2Ds);
            _allBarSprites.Insert(((ItemFactory)itemFactory2D).CurrentIndex, itemFactory2D);

            // Match
            CheckMatch((int)((ItemFactory)itemFactory2D).FactoryType, out var matchs, out _isMatch);
            if (_isMatch)
            {
                itemFactory2D.Match(_itemSlots, _mainCamera, matchs);
            }

            // Jump: tất cả sprites bị dịch chuyển
            for (int i = 0; i < _allBarSprites.Count; i++)
            {
                var item2D = _allBarSprites[i];
                if (item2D == null) continue;
                var itemComp = (ItemFactory)item2D;
                if (itemComp.CurrentIndex != i)
                {
                    var numJump = Mathf.Abs(itemComp.CurrentIndex - i);
                    itemComp.CurrentIndex = i;
                    item2D.JumpOnBar(_itemSlots, _mainCamera, numJump);
                }
            }
        }

        private void CheckMatch(int type, out List<IItemFactory2D> matchs, out bool isMatch)
        {
            matchs = new List<IItemFactory2D>();
            var matchIndices = new List<int>();
            isMatch = false;

            for (int i = 0; i < _data2Ds.Count; i++)
            {
                if (_data2Ds[i] == type)
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
                    _data2Ds.RemoveAt(matchIndices[i]);
                    _allBarSprites.RemoveAt(matchIndices[i]);
                }
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
    }
}
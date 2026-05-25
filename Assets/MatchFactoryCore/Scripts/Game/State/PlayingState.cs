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
        readonly List<GameObject> _itemSlots; // List UI là slot của Sprite
        readonly List<int> _data2Ds;
        readonly List<IItemFactory2D> _allBarSprites;

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

            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                var pos = Touchscreen.current.primaryTouch.position.ReadValue();
                var ray = _mainCamera.ScreenPointToRay(pos);
                Controller.Input.HandleClick(ray, out var go);

                if (go == null) return;
                ItemFactory itemFactory = go.GetComponent<ItemFactory>();
                IItemFactory3D itemFactory3D = itemFactory as IItemFactory3D;
                IItemFactory2D itemFactory2D = itemFactory as IItemFactory2D;
                itemFactory3D.JumpFromBoard(itemFactory3D.Prefab.transform.position + new Vector3(0, 5, -1));
                itemFactory3D.ChangeTo2D();

                CheckLevelTarget(go);

                // Move
                itemFactory2D.MoveToBar(_itemSlots, _mainCamera, _data2Ds);
                _allBarSprites.Insert(itemFactory.CurrentIndex, itemFactory2D);

                // Match
                CheckMatch((int)itemFactory.FactoryType, out var matchs, out var isMatch);
                if (isMatch)
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

                // Win/Lose
                if (IsWin())
                {
                    StateMachine.ChangeState(MatchFactoryState.Win);
                }

                if (IsLose(isMatch))
                {
                    StateMachine.ChangeState(MatchFactoryState.Lose);
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
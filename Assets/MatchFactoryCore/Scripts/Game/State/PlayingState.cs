using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
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

            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                var pos = Touchscreen.current.primaryTouch.position.ReadValue();
                var ray = _mainCamera.ScreenPointToRay(pos);
                Controller.Input.HandleClick(ray, out var go);

                if (go == null) return;
                ItemFactory itemFactory = go.GetComponent<ItemFactory>();
                IItemFactory3D itemFactory3D = itemFactory as IItemFactory3D;
                IItemFactory2D itemFactory2D = itemFactory as IItemFactory2D;
                itemFactory3D.JumpFromBoard(go.transform.position + new Vector3(0, 5, -1));
                itemFactory3D.ChangeTo2D();

                CheckLevelTarget(go, Controller.TargetDictionary);

                // Move
                itemFactory2D.MoveToBar(itemFactory2D.Sprite, _itemSlots, _mainCamera, _data2Ds,
                    (int)itemFactory.FactoryType, _allBarSprites);
                _allBarSprites.Insert(itemFactory.CurrentIndex, itemFactory2D);

                // Match
                CheckMatch((int)itemFactory.FactoryType, _data2Ds, _allBarSprites, out var matchs, out var isMatch);
                if (isMatch)
                {
                    itemFactory2D.Match(_itemSlots, _mainCamera, matchs);

                    for (int i = 0; i < _allBarSprites.Count; i++)
                    {
                        var comp = _allBarSprites[i];
                        if (comp != null)
                        {
                            comp.CurrentIndex = i;
                        }
                    }
                }

                // Jump: tất cả sprites bị dịch chuyển
                for (int i = _allBarSprites.Count - 1; i >= 0; i--)
                {
                    var item2D = _allBarSprites[i];
                    if (item2D == null) return;
                    // if (item2D.LastIndex == item2D.CurrentIndex) continue;
                    // var numJump = Mathf.Abs(item2D.CurrentIndex - item2D.LastIndex);
                    // item2D.JumpOnBar(_allBarSprites[i], _itemSlots, _mainCamera, numJump);
                }

                // Win/Lose
                if (IsWin(Controller.TargetDictionary))
                {
                    StateMachine.ChangeState(MatchFactoryState.Win);
                }

                if (IsLose(1, _allBarSprites, isMatch))
                {
                    StateMachine.ChangeState(MatchFactoryState.Lose);
                }
            }
        }

        private void CheckMatch(int type, List<int> data2Ds, List<IItemFactory2D> allBarSprites,
            out List<IItemFactory2D> matchs,
            out bool isMatch)
        {
            matchs = new List<IItemFactory2D>();
            var matchIndices = new List<int>();
            isMatch = false;

            for (int i = 0; i < data2Ds.Count; i++)
            {
                if (data2Ds[i] == type)
                {
                    matchs.Add(allBarSprites[i]);
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
                    data2Ds.RemoveAt(matchIndices[i]);
                    allBarSprites.RemoveAt(matchIndices[i]);
                }
            }
        }

        void CheckLevelTarget(GameObject go, Dictionary<ItemFactoryType, int> targetDict)
        {
            var itemFactory = go.GetComponent<ItemFactory>();
            if (itemFactory == null) return;

            var type = itemFactory.FactoryType;
            if (targetDict.TryGetValue(type, out var currentTarget))
            {
                if (currentTarget > 0)
                {
                    Controller.ClickTarget(targetDict[type]--);
                }
            }
        }

        bool IsWin(Dictionary<ItemFactoryType, int> targetDict)
        {
            bool isWin = true;
            foreach (var item in targetDict)
            {
                if (item.Value != 0)
                {
                    isWin = false;
                    break;
                }
            }

            return isWin;
        }

        bool IsLose(float time, List<GameObject> allBarSprites, bool isMatch)
        {
            bool isLose = false;
            if (time <= 0)
            {
                isLose = true;
            }
            else if (allBarSprites.Count == MatchFactoryController.MaxSlot && !isMatch)
            {
                isLose = true;
            }

            return isLose;
        }
    }
}
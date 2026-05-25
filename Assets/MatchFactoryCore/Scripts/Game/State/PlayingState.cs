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
        private readonly List<GameObject> _itemSlots;
        private readonly List<int> _data2Ds;
        private readonly List<GameObject> _allBarSprites;

        public PlayingState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(
            controller, stateMachine)
        {
            _itemSlots = Controller.ItemSlots;
            _data2Ds = new List<int>();
            _allBarSprites = new List<GameObject>();
        }

        public override void OnEnter()
        {
            Debug.Log("Enter Playing State");
            _mainCamera = Camera.main;
            Controller.ActiveInput(true);
        }

        public override void OnUpdate()
        {
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                var pos = Touchscreen.current.primaryTouch.position.ReadValue();
                var ray = _mainCamera.ScreenPointToRay(pos);
                Controller.Input.HandleClick(ray, out var go);

                if (go == null) return;
                ItemFactory itemFactory = go.GetComponent<ItemFactory>();
                IItemFactory3D itemFactory3D = go.GetComponent<IItemFactory3D>();
                itemFactory3D.JumpFromBoard(go.transform.position + new Vector3(0, 5, -1));
                itemFactory3D.ChangeTo2D(out var sprite);

                // Move
                if (sprite == null) return;
                IItemFactory2D itemFactory2D = sprite.GetComponent<IItemFactory2D>();
                itemFactory2D.MoveToBar(sprite, _itemSlots, _mainCamera, _data2Ds, (int)itemFactory.Type,
                    _allBarSprites);
                _allBarSprites.Insert(itemFactory2D.CurrentIndex, sprite);

                // Match
                CheckMatch((int)itemFactory.Type, _data2Ds, _allBarSprites, out var matchs, out var isMatch);
                if (isMatch)
                {
                    itemFactory2D.Match(_itemSlots, _mainCamera, matchs);

                    for (int i = 0; i < _allBarSprites.Count; i++)
                    {
                        var comp = _allBarSprites[i].GetComponent<IItemFactory2D>();
                        if (comp != null)
                        {
                            comp.CurrentIndex = i;
                        }
                    }
                }

                // Jump: tất cả sprites bị dịch chuyển
                for (int i = _allBarSprites.Count - 1; i >= 0; i--)
                {
                    var item2D = _allBarSprites[i].GetComponent<IItemFactory2D>();
                    if (item2D == null) return;
                    if (item2D.LastIndex == item2D.CurrentIndex) continue;
                    var numJump = Mathf.Abs(item2D.CurrentIndex - item2D.LastIndex);
                    item2D.JumpOnBar(_allBarSprites[i], _itemSlots, _mainCamera, numJump);
                }
            }
        }

        void CheckMatch(int type, List<int> data2Ds, List<GameObject> allBarSprites, out List<GameObject> matchs,
            out bool isMatch)
        {
            matchs = new List<GameObject>();
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
    }
}
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
        private List<GameObject> _itemSlots;
        private List<IItemFactory2D> _item2Ds;
        private List<IItemFactory3D> _item3Ds;
        private List<int> _data2Ds;
        private List<GameObject> _go2Ds;

        public PlayingState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(
            controller, stateMachine)
        {
            _itemSlots = Controller.ItemSlots;
            _item2Ds = Controller.Item2Ds;
            _item3Ds = Controller.Item3Ds;
            _data2Ds = new List<int>();
            _go2Ds = new List<GameObject>();
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
                itemFactory2D.MoveToBar(sprite, _itemSlots, _mainCamera, _data2Ds, (int)itemFactory.Type, out _go2Ds);

                // Jump
                if (_go2Ds.Count == 0) return;
                for (int i = _go2Ds.Count - 1; i >= 0; i--)
                {
                    var itemSprite = _go2Ds[i].GetComponent<ItemSprite>();
                    if (itemSprite == null) return;
                    if (itemSprite.LastIndex == itemSprite.CurrentIndex) continue;
                    var numJump = itemSprite.CurrentIndex - itemSprite.LastIndex;
                    itemFactory2D.JumpOnBar(_go2Ds[i], _itemSlots, _mainCamera, numJump);
                }
            }
        }
    }
}
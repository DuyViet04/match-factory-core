using System;
using MatchFactoryCore.Scripts.Item;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MatchFactoryCore.Scripts.Game
{
    public class ControllerItemFactory3D : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;

        public event Action<int> OnPointerReleased;

        private const string ItemFactory3DLayer = "ItemFactory";
        private const float DragThreshold = 15f;

        Vector2 _pointerPos;
        RaycastHit _hit;
        ItemFactory _itemFactory;
        IItemFactory3D _itemFactory3D;

        Vector2 _startMousePos;
        bool _isPressing;
        bool _hasMovedEnoughForDrag;
        int _lastId;
        IItemFactory3D _lastItemFactory3D;

        public void UpdateRaycast()
        {
            // if (Pointer.current.press.wasPressedThisFrame || Pointer.current.press.wasReleasedThisFrame)
            if (Pointer.current.press.isPressed)
            {
                _pointerPos = Pointer.current.position.ReadValue();
                Physics.Raycast(mainCamera.ScreenPointToRay(_pointerPos), out _hit, int.MaxValue,
                    LayerMask.GetMask(ItemFactory3DLayer));
                if (_hit.collider == null) return;
                GameObject go = _hit.collider.gameObject;
                _itemFactory = go.GetComponent<ItemFactory>();
                _itemFactory3D = _itemFactory as IItemFactory3D;
            }
            // else return;

            if (Pointer.current.press.wasPressedThisFrame)
            {
                _startMousePos = _pointerPos;
                _isPressing = true;
                _hasMovedEnoughForDrag = false;
                _lastId = -1;
                _lastItemFactory3D = null;

                _itemFactory3D.ObjectOutline.enabled = true;
                _itemFactory3D.ObjectRigidbody.AddForce(Vector3.up, ForceMode.Impulse);

                _lastItemFactory3D = _itemFactory3D;
            }

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                if (_lastItemFactory3D != null)
                    _lastItemFactory3D.ObjectOutline.enabled = false;

                if (!_hasMovedEnoughForDrag)
                {
                    HandleItemFactory3D(_itemFactory3D, _itemFactory.Id);
                }

                _isPressing = false;
                _lastId = -1;
                _lastItemFactory3D = null;
            }

            if (_isPressing)
            {
                if (!_hasMovedEnoughForDrag)
                {
                    if (Vector2.Distance(_startMousePos, _pointerPos) > DragThreshold)
                    {
                        _hasMovedEnoughForDrag = true;
                    }
                }

                if (_hasMovedEnoughForDrag && _itemFactory.Id != _lastId)
                {
                    _itemFactory3D.ObjectRigidbody.AddForce(Vector3.up, ForceMode.Impulse);
                    _itemFactory3D.ObjectOutline.enabled = true;

                    if (_lastItemFactory3D != null)
                    {
                        _itemFactory3D.ObjectOutline.enabled = false;

                        _lastItemFactory3D = _itemFactory3D;
                    }

                    _lastId = _itemFactory.Id;
                }
            }
        }

        private void HandleItemFactory3D(IItemFactory3D itemFactory3D, int id)
        {
            OnPointerReleased?.Invoke(id);
        }
    }
}
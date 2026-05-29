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

        private int _layerMask;
        private Vector2 _lastPointerPos;
        private Vector2 _lastRaycastPointerPos;

        Vector2 _pointerPos;
        RaycastHit _hit;
        ItemFactory _itemFactory;
        IItemFactory3D _itemFactory3D;

        Vector2 _startMousePos;
        bool _isPressing;
        bool _hasMovedEnoughForDrag;
        int _lastId;
        IItemFactory3D _lastItemFactory3D;

        private void Awake()
        {
            _layerMask = LayerMask.GetMask(ItemFactory3DLayer);
        }

        public void UpdateRaycast()
        {
            if (Pointer.current == null) return;

            Vector2 currentPointerPos = Pointer.current.position.ReadValue();
            bool wasPressed = Pointer.current.press.wasPressedThisFrame;
            bool wasReleased = Pointer.current.press.wasReleasedThisFrame;

            bool shouldRaycast = wasPressed || wasReleased || (_isPressing && _hasMovedEnoughForDrag && currentPointerPos != _lastRaycastPointerPos);

            bool hitSomething = false;
            ItemFactory hitItemFactory = null;
            IItemFactory3D hitItemFactory3D = null;

            if (shouldRaycast)
            {
                _lastRaycastPointerPos = currentPointerPos;
                Ray ray = mainCamera.ScreenPointToRay(currentPointerPos);
                if (Physics.Raycast(ray, out _hit, float.MaxValue, _layerMask))
                {
                    GameObject go = _hit.collider.gameObject;
                    hitItemFactory = go.GetComponent<ItemFactory>();
                    if (hitItemFactory != null)
                    {
                        hitItemFactory3D = hitItemFactory as IItemFactory3D;
                        hitSomething = true;
                    }
                }
            }

            if (wasPressed)
            {
                _pointerPos = currentPointerPos;
                _startMousePos = currentPointerPos;
                _isPressing = true;
                _hasMovedEnoughForDrag = false;
                _lastId = -1;
                _lastItemFactory3D = null;

                if (hitSomething)
                {
                    _itemFactory = hitItemFactory;
                    _itemFactory3D = hitItemFactory3D;

                    _itemFactory3D.ObjectOutline.enabled = true;
                    _itemFactory3D.ObjectRigidbody.AddForce(Vector3.up, ForceMode.Impulse);

                    _lastItemFactory3D = _itemFactory3D;
                    _lastId = _itemFactory.Id;
                }
                else
                {
                    _itemFactory = null;
                    _itemFactory3D = null;
                }
            }

            if (wasReleased)
            {
                if (hitSomething)
                {
                    _itemFactory = hitItemFactory;
                    _itemFactory3D = hitItemFactory3D;
                }

                if (_lastItemFactory3D != null)
                {
                    _lastItemFactory3D.ObjectOutline.enabled = false;
                }

                if (!_hasMovedEnoughForDrag && _itemFactory != null && _itemFactory3D != null)
                {
                    HandleItemFactory3D(_itemFactory3D, _itemFactory.Id);
                }

                _isPressing = false;
                _hasMovedEnoughForDrag = false;
                _lastItemFactory3D = null;
                _lastId = -1;
                _itemFactory = null;
                _itemFactory3D = null;
            }

            if (_isPressing && !wasPressed && !wasReleased)
            {
                if (!_hasMovedEnoughForDrag)
                {
                    if (Vector2.Distance(_startMousePos, currentPointerPos) > DragThreshold)
                    {
                        _hasMovedEnoughForDrag = true;
                    }
                }

                if (_hasMovedEnoughForDrag && shouldRaycast)
                {
                    if (hitSomething)
                    {
                        if (hitItemFactory.Id != _lastId)
                        {
                            if (_lastItemFactory3D != null && _lastItemFactory3D != hitItemFactory3D)
                            {
                                _lastItemFactory3D.ObjectOutline.enabled = false;
                            }

                            hitItemFactory3D.ObjectOutline.enabled = true;
                            hitItemFactory3D.ObjectRigidbody.AddForce(Vector3.up, ForceMode.Impulse);

                            _itemFactory = hitItemFactory;
                            _itemFactory3D = hitItemFactory3D;
                            _lastItemFactory3D = hitItemFactory3D;
                            _lastId = hitItemFactory.Id;
                        }
                    }
                    else
                    {
                        if (_lastItemFactory3D != null)
                        {
                            _lastItemFactory3D.ObjectOutline.enabled = false;
                            _lastItemFactory3D = null;
                        }
                        _lastId = -1;
                        _itemFactory = null;
                        _itemFactory3D = null;
                    }
                }
            }

            _lastPointerPos = currentPointerPos;
        }

        private void HandleItemFactory3D(IItemFactory3D itemFactory3D, int id)
        {
            OnPointerReleased?.Invoke(id);
        }
    }
}
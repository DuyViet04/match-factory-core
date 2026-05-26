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
        public event Action<Vector3> OnJumpOnBoardComplete;

        private const string ItemFactory3DLayer = "ItemFactory";
        private const float DragThreshold = 15f;

        Vector2 _startMousePos;
        bool _isPressing;
        bool _hasMovedEnoughForDrag;
        int _lastDragId;
        private GameObject _lastItemGameObject;

        public void OnUpdate()
        {
            var pointerPos = Pointer.current.position.ReadValue();
            var rayCamera = mainCamera.ScreenPointToRay(pointerPos);
            var isHit = Physics.Raycast(rayCamera, out RaycastHit hit, int.MaxValue,
                LayerMask.GetMask(ItemFactory3DLayer));
            if (!isHit || hit.collider == null) return;
            var itemFactoryGo = hit.collider.gameObject;

            if (Pointer.current.press.wasPressedThisFrame)
            {
                _startMousePos = pointerPos;
                _isPressing = true;
                _hasMovedEnoughForDrag = false;
                _lastDragId = -1;
                _lastItemGameObject = null;

                var outline = itemFactoryGo.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = true;
                }

                var itemFactoryEntity = itemFactoryGo.GetComponent<ItemFactory>();
                var itemFactory3D = itemFactoryGo.GetComponent<IItemFactory3D>();
                itemFactory3D.RigidbodyObject.AddForce(Vector3.up, ForceMode.Impulse);
                _lastDragId = itemFactoryEntity.Id;
                _lastItemGameObject = itemFactoryGo;
            }

            if (_isPressing)
            {
                if (!_hasMovedEnoughForDrag)
                {
                    if (Vector2.Distance(_startMousePos, pointerPos) > DragThreshold)
                    {
                        _hasMovedEnoughForDrag = true;
                    }
                }

                if (_hasMovedEnoughForDrag)
                {
                    var itemFactoryEntity = itemFactoryGo.GetComponent<ItemFactory>();
                    var itemFactory3D = itemFactoryGo.GetComponent<IItemFactory3D>();
                    if (itemFactoryEntity.Id != _lastDragId)
                    {
                        itemFactory3D.RigidbodyObject.AddForce(Vector3.up, ForceMode.Impulse);
                        _lastDragId = itemFactoryEntity.Id;

                        var outline = itemFactoryGo.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.enabled = true;
                        }

                        if (_lastItemGameObject != null)
                        {
                            var lastOutline = _lastItemGameObject.GetComponent<Outline>();
                            if (lastOutline != null)
                            {
                                lastOutline.enabled = false;
                            }

                            _lastItemGameObject = itemFactoryGo;
                        }
                    }
                }
            }

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                _isPressing = false;

                if (_lastItemGameObject != null)
                {
                    var outline = _lastItemGameObject.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }

                if (!_hasMovedEnoughForDrag)
                {
                    var itemFactoryEntity = itemFactoryGo.GetComponent<ItemFactory>();
                    var itemFactory3D = itemFactoryGo.GetComponent<IItemFactory3D>();
                    OnPointerReleased?.Invoke(itemFactoryEntity.Id);

                    HandleItemFactory3D(itemFactory3D);
                }
            }
        }

        private void HandleItemFactory3D(IItemFactory3D itemFactory3D)
        {
            var jumpTargetPos = itemFactory3D.Prefab.transform.position + new Vector3(0, 5, -1);
            itemFactory3D.JumpFromBoard(jumpTargetPos,
                () =>
                {
                    itemFactory3D.ChangeTo2D();
                    OnJumpOnBoardComplete?.Invoke(jumpTargetPos);
                });
        }
    }
}
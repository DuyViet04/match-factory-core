using System;
using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Item;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace MatchFactoryCore.Scripts.Game
{
    public class ControllerItemFactory3D : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private InfoItemsMatch3Factory infoItemsMatch3Factory;
        [SerializeField] private InfoLevelsMatch3Factory infoLevelsMatch3Factory;
        [SerializeField] private GameObject holder;

        public float spawnInHighValue;
        public float maxX, maxZ;

        public event Action<int> OnPointerReleased;

        private const string ItemFactory3DLayer = "ItemFactory";
        private const float DragThreshold = 15f;
        private readonly Dictionary<int, ItemFactory> _dictItemFactory = new Dictionary<int, ItemFactory>();
        private readonly Dictionary<ItemFactoryType, int> _targetDictionary = new Dictionary<ItemFactoryType, int>();
        private int _layerMask;

        Vector2 _lastRaycastPointerPos;
        Vector2 _startMousePos;
        RaycastHit _hit;
        ItemFactory _itemFactory;
        IItem3D _item3D;
        IItem3D _lastItem3D;
        bool _isPressing;
        bool _hasMovedEnoughForDrag;
        int _lastId;

        private void Awake()
        {
            _layerMask = LayerMask.GetMask(ItemFactory3DLayer);
            InitializeDictionary();
        }
        
        private void InitializeDictionary()
        {
            infoItemsMatch3Factory.SetCache();
        }

        public void SpawnAllItem(int level, Action onReady)
        {
            var dataLevel = infoLevelsMatch3Factory.CacheDictInfoLevelsMatch3Factory[level];
            var levelTarget = dataLevel.DictLevelTarget;
            var otherObjInLevel = dataLevel.DictOtherObjectInLevel;

            // Spawn item cần thu thập
            foreach (var item in levelTarget)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    Spawn(item.Key, i);
                }

                _targetDictionary.TryAdd(item.Key, item.Value);
            }

            // Spawn item khác
            foreach (var item in otherObjInLevel)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    Spawn(item.Key, i);
                }
            }

            onReady?.Invoke();
        }

        public void UpdateRaycast()
        {
            if (Pointer.current == null) return;

            Vector2 currentPointerPos = Pointer.current.position.ReadValue();
            bool wasPressed = Pointer.current.press.wasPressedThisFrame;
            bool wasReleased = Pointer.current.press.wasReleasedThisFrame;

            bool shouldRaycast = wasPressed || wasReleased ||
                                 (_isPressing && _hasMovedEnoughForDrag && currentPointerPos != _lastRaycastPointerPos);

            bool hitSomething = false;
            ItemFactory hitItemFactory = null;
            IItem3D hitItem3D = null;

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
                        hitItem3D = hitItemFactory as IItem3D;
                        hitSomething = true;
                    }
                }
            }

            if (wasPressed)
            {
                _startMousePos = currentPointerPos;
                _isPressing = true;
                _hasMovedEnoughForDrag = false;
                _lastId = -1;
                _lastItem3D = null;

                if (hitSomething)
                {
                    _itemFactory = hitItemFactory;
                    _item3D = hitItem3D;

                    _item3D.ObjectOutline.enabled = true;
                    _item3D.ObjectRigidbody.AddForce(Vector3.up, ForceMode.Impulse);

                    _lastItem3D = _item3D;
                    _lastId = _itemFactory.Id;
                }
                else
                {
                    _itemFactory = null;
                    _item3D = null;
                }
            }

            if (wasReleased)
            {
                if (hitSomething)
                {
                    _itemFactory = hitItemFactory;
                    _item3D = hitItem3D;
                }

                if (_lastItem3D != null)
                {
                    _lastItem3D.ObjectOutline.enabled = false;
                }

                if (!_hasMovedEnoughForDrag && _itemFactory != null && _item3D != null)
                {
                    HandleItemFactory3D(_item3D, _itemFactory.Id);
                }

                _isPressing = false;
                _hasMovedEnoughForDrag = false;
                _lastItem3D = null;
                _lastId = -1;
                _itemFactory = null;
                _item3D = null;
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
                            if (_lastItem3D != null && _lastItem3D != hitItem3D)
                            {
                                _lastItem3D.ObjectOutline.enabled = false;
                            }

                            hitItem3D.ObjectOutline.enabled = true;
                            hitItem3D.ObjectRigidbody.AddForce(Vector3.up, ForceMode.Impulse);

                            _itemFactory = hitItemFactory;
                            _item3D = hitItem3D;
                            _lastItem3D = hitItem3D;
                            _lastId = hitItemFactory.Id;
                        }
                    }
                    else
                    {
                        if (_lastItem3D != null)
                        {
                            _lastItem3D.ObjectOutline.enabled = false;
                            _lastItem3D = null;
                        }

                        _lastId = -1;
                        _itemFactory = null;
                        _item3D = null;
                    }
                }
            }
        }

        private void HandleItemFactory3D(IItem3D item3D, int id)
        {
            OnPointerReleased?.Invoke(id);
        }
        
        public void RemoveItemFactory(List<int> idList)
        {
            for (int i = 0; i < idList.Count; i++)
            {
                Destroy(_dictItemFactory[idList[i]].gameObject);
                _dictItemFactory.Remove(idList[i]);
            }
        }

        #region Spawn Helper

        void Spawn(ItemFactoryType itemFactoryType, int index)
        {
            infoItemsMatch3Factory.CacheDictInfoItemsMatch3Factory.TryGetValue(itemFactoryType, out var so);
            if (so == null)
            {
                Debug.LogError($"{itemFactoryType} not found");
                return;
            }

            GameObject newItemFactory3D = Instantiate(so.prefab, GetRandomSpawnPoint(), Quaternion.identity);
            newItemFactory3D.transform.parent = holder.transform;
            ItemFactory itemFactoryComp = newItemFactory3D.GetComponent<ItemFactory>();

            InitItemFactory3DContext initItemFactory3DContext = new InitItemFactory3DContext()
            {
                Id = (int)itemFactoryType + index,
                Prefab = newItemFactory3D,
                PrefabSize = so.prefabSize,
                Sprite = so.sprite,
                ItemFactoryType = itemFactoryType,
                SpriteScaleOnBar = so.spriteScaleOnBar,
            };
            itemFactoryComp.Initialize(initItemFactory3DContext);
            _dictItemFactory.TryAdd(initItemFactory3DContext.Id, itemFactoryComp);
        }

        Vector3 GetRandomSpawnPoint()
        {
            float randX = Random.Range(-maxX, maxX);
            float randZ = Random.Range(-maxZ, maxZ);
            Vector3 randomSpawnPoint = new Vector3(randX, spawnInHighValue, randZ);
            return randomSpawnPoint;
        }

        #endregion

        #region Gets Sets

        public DataLevelMatchFactory GetDataLevel(int level)
        {
            return infoLevelsMatch3Factory.CacheDictInfoLevelsMatch3Factory[level];
        }

        public Dictionary<ItemFactoryType, int> GetTargetDictionary()
        {
            return _targetDictionary;
        }

        public Dictionary<int, ItemFactory> GetDictItemFactory()
        {
            return _dictItemFactory;
        }

        #endregion
    }
}
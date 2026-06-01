using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<int, ItemAction> _dictItemAction = new Dictionary<int, ItemAction>();
        private readonly Dictionary<ItemFactoryType, int> _targetDictionary = new Dictionary<ItemFactoryType, int>();
        private readonly Dictionary<ItemFactoryType, int> _otherItemDictionary = new Dictionary<ItemFactoryType, int>();
        private int _layerMask;

        Vector2 _lastRaycastPointerPos;
        Vector2 _startMousePos;
        RaycastHit _hit;
        ItemFactory _itemFactory;
        ItemAction _itemAction;
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
            ItemAction hitItemAction = null;
            IItem3D hitItem3D = null;

            if (shouldRaycast)
            {
                _lastRaycastPointerPos = currentPointerPos;
                Ray ray = mainCamera.ScreenPointToRay(currentPointerPos);
                if (Physics.Raycast(ray, out _hit, float.MaxValue, _layerMask))
                {
                    GameObject go = _hit.collider.gameObject;
                    hitItemFactory = go.GetComponent<ItemFactory>();
                    hitItemAction = go.GetComponent<ItemAction>();

                    if (hitItemFactory != null)
                    {
                        hitItem3D = hitItemFactory as IItem3D;
                        hitSomething = true;
                    }

                    if (hitItemAction != null)
                    {
                        hitItem3D = hitItemAction as IItem3D;
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
                    _itemAction = hitItemAction;
                    _item3D = hitItem3D;

                    _item3D.ObjectOutline.enabled = true;
                    _item3D.ObjectRigidbody.AddForce(Vector3.up, ForceMode.Impulse);

                    if (_itemFactory != null)
                    {
                        _lastItem3D = _item3D;
                        _lastId = _itemFactory.Id;
                    }
                }
                else
                {
                    _itemFactory = null;
                    _itemAction = null;
                    _item3D = null;
                }
            }

            if (wasReleased)
            {
                if (hitSomething)
                {
                    _itemFactory = hitItemFactory;
                    _itemAction = hitItemAction;
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

                if (!_hasMovedEnoughForDrag && _itemAction != null && _item3D != null)
                {
                    HandleItemAction(_item3D, _itemAction.Id);
                }

                _isPressing = false;
                _hasMovedEnoughForDrag = false;
                _lastItem3D = null;
                _lastId = -1;
                _itemFactory = null;
                _itemAction = null;
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
                            _itemAction = hitItemAction;
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
                        _itemAction = null;
                        _item3D = null;
                    }
                }
            }
        }

        private void HandleItemFactory3D(IItem3D item3D, int id)
        {
            ActionType actionType = item3D.ActionType;
            if (actionType == ActionType.Normal)
            {
                OnPointerReleased?.Invoke(id);
            }
        }

        private void HandleItemAction(IItem3D item3D, int id)
        {
            ActionType actionType = item3D.ActionType;
            switch (actionType)
            {
                case ActionType.Firework:
                    HandleItemActionFirework(id);
                    break;
            }
        }

        private void HandleItemActionFirework(int id)
        {
            List<ItemFactoryType> typeList = new List<ItemFactoryType>();
            foreach (var item in _otherItemDictionary)
            {
                typeList.Add(item.Key);
            }

            while (typeList.Count > 0)
            {
                int randIndex = Random.Range(0, typeList.Count);
                ItemFactoryType itemType = typeList[randIndex];
                int itemTypeCount = _otherItemDictionary[itemType];

                if (itemTypeCount < 3)
                {
                    typeList.Remove(itemType);
                }
                else
                {
                    // Lấy list ItemFactory có type = type random
                    List<ItemFactory> itemFactoryList = _dictItemFactory
                        .Where(item => item.Key - item.Key % (int)itemType == (int)itemType)
                        .Select(itemFactory => itemFactory.Value).ToList();

                    List<ItemFactory> itemFactoryTargets = new List<ItemFactory>();
                    int counting = 0;
                    for (int i = itemFactoryList.Count - 1; i >= 0; i--)
                    {
                        itemFactoryTargets.Add(itemFactoryList[i]);
                        counting++;

                        if (counting == 3) break;
                    }

                    _dictItemAction.TryGetValue(id, out ItemAction firework);
                    if (firework != null)
                    {
                        List<ItemAction> fireworkList = new List<ItemAction> { firework };
                        for (int i = 0; i < 2; i++)
                        {
                            ItemAction cloneFirework = Instantiate(firework, firework.transform.position,
                                firework.transform.rotation);
                            InitItem3DContext cloneContext = new InitItem3DContext()
                            {
                                ActionType = firework.ActionType,
                                Prefab = firework.Prefab,
                                PrefabSize = firework.PrefabSize,
                            };
                            cloneFirework.InitializeItemAction(cloneContext);
                            fireworkList.Add(cloneFirework);
                        }

                        for (int i = 0; i < itemFactoryTargets.Count; i++)
                        {
                            fireworkList[i].ActionBehaviour(itemFactoryTargets[i].transform.position);
                            Debug.Log(itemFactoryTargets[i].Id);
                        }
                    }

                    break;
                }
            }
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

        public void SpawnAllItem(int level, Action onReady)
        {
            DataLevelMatchFactory dataLevel = infoLevelsMatch3Factory.CacheDictInfoLevelsMatch3Factory[level];
            Dictionary<ItemFactoryType, int> levelTarget = dataLevel.CacheDictLevelTarget;
            Dictionary<ItemFactoryType, int> otherObjInLevel = dataLevel.CacheDictOtherObjectInLevel;
            Dictionary<ActionType, int> dictItemAction = dataLevel.CacheDictItemAction;

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

                _otherItemDictionary.TryAdd(item.Key, item.Value);
            }

            foreach (var itemAction in dictItemAction)
            {
                for (int i = 0; i < itemAction.Value; i++)
                {
                    infoItemsMatch3Factory.CacheDictInfoItemActionsMatch3Factory.TryGetValue(itemAction.Key,
                        out DataItemFactory dataItemAction);
                    if (dataItemAction == null)
                    {
                        Debug.LogError($"{itemAction} not found");
                        return;
                    }

                    GameObject newItemAction =
                        Instantiate(dataItemAction.prefab, GetRandomSpawnPoint(), Quaternion.identity);
                    newItemAction.transform.SetParent(holder.transform);
                    ItemAction itemActionComp = newItemAction.GetComponent<ItemAction>();

                    InitItem3DContext itemActionContext = new InitItem3DContext()
                    {
                        Id = (int)itemAction.Key * 10 + i,
                        ActionType = itemAction.Key,
                        Prefab = dataItemAction.prefab,
                        PrefabSize = dataItemAction.prefabSize
                    };
                    itemActionComp.InitializeItemAction(itemActionContext);
                    _dictItemAction.TryAdd(itemActionContext.Id, itemActionComp);
                }
            }

            onReady?.Invoke();
        }

        void Spawn(ItemFactoryType itemFactoryType, int index)
        {
            infoItemsMatch3Factory.CacheDictInfoItemsMatch3Factory.TryGetValue(itemFactoryType, out DataItemFactory so);
            if (so == null)
            {
                Debug.LogError($"{itemFactoryType} not found");
                return;
            }

            GameObject newItemFactory3D = Instantiate(so.prefab, GetRandomSpawnPoint(), Quaternion.identity);
            newItemFactory3D.transform.parent = holder.transform;
            ItemFactory itemFactoryComp = newItemFactory3D.GetComponent<ItemFactory>();

            InitItem3DContext initItem3DContext = new InitItem3DContext()
            {
                Id = (int)itemFactoryType + index,
                Prefab = newItemFactory3D,
                PrefabSize = so.prefabSize,
                ActionType = so.actionType,
                Sprite = so.sprite,
                ItemFactoryType = itemFactoryType,
                SpriteScaleOnBar = so.spriteScaleOnBar,
            };
            itemFactoryComp.Initialize(initItem3DContext);
            _dictItemFactory.TryAdd(initItem3DContext.Id, itemFactoryComp);
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
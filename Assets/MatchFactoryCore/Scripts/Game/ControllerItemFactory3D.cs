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
        public event Action<Dictionary<ItemFactoryType, int>> OnRemainTargetChanged;
        public event Action<int> OnItemActionHourglassUse;

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

        // TODO: Chỉ làm việc với IItem3D
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
                        hitItem3D = hitItemFactory;
                        hitSomething = true;
                    }

                    if (hitItemAction != null)
                    {
                        hitItem3D = hitItemAction;
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

                    _lastItem3D = _item3D;
                    if (_itemFactory != null)
                    {
                        _lastId = _itemFactory.Id;
                    }
                    else if (_itemAction != null)
                    {
                        _lastId = _itemAction.Id;
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
                        int hitId = hitItemFactory != null ? hitItemFactory.Id : hitItemAction.Id;
                        if (hitId != _lastId)
                        {
                            if (_lastItem3D != null && _lastItem3D != hitItem3D)
                            {
                                _lastItem3D.ObjectOutline.enabled = false;
                            }

                            if (hitItem3D.ObjectOutline != null)
                                hitItem3D.ObjectOutline.enabled = true;
                            hitItem3D.ObjectRigidbody.AddForce(Vector3.up, ForceMode.Impulse);

                            _itemFactory = hitItemFactory;
                            _itemAction = hitItemAction;
                            _item3D = hitItem3D;
                            _lastItem3D = hitItem3D;
                            _lastId = hitId;
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
                CheckItemChoose(id);
            }
        }

        private void CheckItemChoose(int id)
        {
            _dictItemFactory.TryGetValue(id, out ItemFactory itemFactory);
            if (itemFactory == null) return;

            bool isTarget = _targetDictionary.TryGetValue(itemFactory.ItemFactoryType, out int remainTarget);
            bool isOther = _otherItemDictionary.TryGetValue(itemFactory.ItemFactoryType, out int remainOther);
            if (isTarget && remainTarget > 0)
            {
                _targetDictionary[itemFactory.ItemFactoryType]--;
                OnRemainTargetChanged?.Invoke(_targetDictionary);
            }

            if (isOther && remainOther > 0)
            {
                _otherItemDictionary[itemFactory.ItemFactoryType]--;
                if (_otherItemDictionary[itemFactory.ItemFactoryType] <= 0)
                {
                    _otherItemDictionary.Remove(itemFactory.ItemFactoryType);
                }
            }
        }

        public void RemoveItemFactory(List<int> idList)
        {
            foreach (var idTemp in idList)
            {
                Destroy(_dictItemFactory[idTemp].gameObject);
                _dictItemFactory.Remove(idTemp);
            }
        }

        public void MoveToVacuum(List<IItem3D> list3D, Vector3 position)
        {
            if (list3D != null && list3D.Count > 0)
            {
                foreach (var item3DTemp in list3D)
                {
                    Vector3 targetPosition = mainCamera.ScreenToWorldPoint(position);
                    targetPosition.y = 0;
                    item3DTemp.JumpToBooster(targetPosition);

                    int item3DId = ((ItemFactory)item3DTemp).Id;
                    _dictItemFactory.Remove(item3DId);
                    _targetDictionary[((ItemFactory)item3DTemp).ItemFactoryType]--;
                    OnRemainTargetChanged?.Invoke(_targetDictionary);
                }
            }
        }

        public void JumpToBoard(int id, Vector3 startPos)
        {
            _dictItemFactory.TryGetValue(id, out ItemFactory itemFactory);
            if (itemFactory != null)
            {
                bool isTarget = CheckIsTargetItemById(id);
                if (isTarget)
                {
                    _targetDictionary[itemFactory.ItemFactoryType]++;
                    OnRemainTargetChanged?.Invoke(_targetDictionary);
                }

                IItem3D item3D = itemFactory;
                Vector3 endPos = GetRandomSpawnPoint();
                endPos.y = mainCamera.transform.position.y / 2f;

                item3D.Prefab.SetActive(true);
                item3D.Prefab.transform.position = startPos;
                item3D.JumpFromBooster(endPos);
            }
        }

        public void BlowByFanBooster()
        {
            foreach (var itemFactoryTemp in _dictItemFactory.Values)
            {
                itemFactoryTemp.BlowByFanBooster(maxX, maxZ);
            }

            foreach (var itemActionTemp in _dictItemAction.Values)
            {
                itemActionTemp.BlowByFanBooster(maxX, maxZ);
            }
        }

        #region Item Action Rule

        private void HandleItemAction(IItem3D item3D, int id)
        {
            ActionType actionType = item3D.ActionType;
            switch (actionType)
            {
                case ActionType.Firework:
                    HandleItemActionFirework(id);
                    break;
                case ActionType.Hourglass:
                    HandleItemActionHourglass(id);
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
                    int idx = 0;
                    while (counting < 3)
                    {
                        if (itemFactoryList[idx].gameObject.activeSelf == false)
                        {
                            idx++;
                            continue;
                        }
                        else
                        {
                            itemFactoryTargets.Add(itemFactoryList[idx]);
                            counting++;
                            idx++;
                        }

                        if (counting == 3) break;
                    }

                    _dictItemAction.TryGetValue(id, out ItemAction firework);
                    if (firework != null)
                    {
                        _dictItemAction.Remove(firework.Id);
                        List<ItemAction> fireworkList = new List<ItemAction> { firework };
                        for (int i = 0; i < 2; i++)
                        {
                            ItemAction cloneFirework = Instantiate(firework, firework.transform.position,
                                firework.transform.rotation);
                            InitItem3DContext cloneContext = new InitItem3DContext()
                            {
                                ActionType = firework.ActionType,
                                Prefab = cloneFirework.gameObject,
                                PrefabSize = firework.PrefabSize,
                            };
                            cloneFirework.InitializeItemAction(cloneContext);
                            fireworkList.Add(cloneFirework);
                        }

                        for (int i = 0; i < itemFactoryTargets.Count; i++)
                        {
                            _dictItemFactory.Remove(itemFactoryTargets[i].Id);
                            int index = i;
                            fireworkList[index].ActionBehaviour(itemFactoryTargets[index].transform.position,
                                () => { itemFactoryTargets[index].Explode(); });
                        }
                    }

                    break;
                }
            }

            if (typeList.Count == 0)
            {
                _dictItemAction.TryGetValue(id, out ItemAction firework);
                if (firework != null)
                {
                    _dictItemAction.Remove(firework.Id);
                    firework.Explode();
                }
            }
        }

        private void HandleItemActionHourglass(int id)
        {
            _dictItemAction.TryGetValue(id, out ItemAction hourglass);
            if (hourglass != null)
            {
                _dictItemAction.Remove(hourglass.Id);
                OnItemActionHourglassUse?.Invoke(10);
                hourglass.Explode();
            }
        }

        #endregion

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
                        Prefab = newItemAction,
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

        public Dictionary<ItemFactoryType, int> GetOtherItemDictionary()
        {
            return _otherItemDictionary;
        }

        public Dictionary<int, ItemFactory> GetDictItemFactory()
        {
            return _dictItemFactory;
        }

        private ItemFactory GetItemFactoryById(int id)
        {
            _dictItemFactory.TryGetValue(id, out ItemFactory item);
            return item;
        }

        public bool CheckIsTargetItemById(int id)
        {
            ItemFactory item = GetItemFactoryById(id);
            return _targetDictionary.TryGetValue(item.ItemFactoryType, out _);
        }

        private Dictionary<ItemFactoryType, List<ItemFactory>> GetDictAllItem()
        {
            Dictionary<ItemFactoryType, List<ItemFactory>> dict = new Dictionary<ItemFactoryType, List<ItemFactory>>();
            foreach (var item in _dictItemFactory)
            {
                if (dict.ContainsKey(item.Value.ItemFactoryType))
                {
                    dict[item.Value.ItemFactoryType].Add(item.Value);
                }
                else
                {
                    dict.Add(item.Value.ItemFactoryType, new List<ItemFactory>());
                    dict[item.Value.ItemFactoryType].Add(item.Value);
                }
            }

            return dict;
        }

        private List<ItemFactory> GetListItemByType(ItemFactoryType type)
        {
            var allItem = GetDictAllItem();
            allItem.TryGetValue(type, out List<ItemFactory> list);
            return list;
        }

        public List<IItem3D> GetListItemRandomByBooster(int count)
        {
            List<IItem3D> result = new List<IItem3D>();
            List<ItemFactory> allTargetItem = GetAllTargetItem();

            if (allTargetItem.Count <= 0)
            {
                return result;
            }

            int randomIndex = Random.Range(0, allTargetItem.Count);
            ItemFactoryType type = allTargetItem[randomIndex].ItemFactoryType;

            List<ItemFactory> typeList = GetListItemByType(type);
            List<ItemFactory> copy = new List<ItemFactory>(typeList);
            for (int i = 0; i < count; i++)
            {
                int randIndex = Random.Range(0, copy.Count);
                result.Add(copy[randIndex]);
                copy.RemoveAt(randIndex);
            }

            return result;
        }

        public List<IItem3D> GetRandomItemsByType(ItemFactoryType type, int count)
        {
            List<IItem3D> result = new List<IItem3D>();
            List<ItemFactory> list = GetListItemByType(type);
            if (list.Count < count)
            {
                Debug.LogError($"{type} not enough items");
                return result;
            }
            else
            {
                List<ItemFactory> copyList = new List<ItemFactory>();
                copyList.AddRange(list);
                for (int i = 0; i < count; i++)
                {
                    int randomIndex = Random.Range(0, copyList.Count);
                    result.Add(copyList[randomIndex]);
                    copyList.RemoveAt(randomIndex);
                }

                return result;
            }
        }

        private List<ItemFactory> GetAllTargetItem()
        {
            List<ItemFactory> result = new List<ItemFactory>();
            foreach (var item in _targetDictionary)
            {
                result.AddRange(GetListItemByType(item.Key));
            }

            return result;
        }

        #endregion
    }
}
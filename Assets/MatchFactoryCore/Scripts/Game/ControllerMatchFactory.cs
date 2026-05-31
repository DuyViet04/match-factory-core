using System;
using System.Collections;
using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game.State;
using MatchFactoryCore.Scripts.Item;
using MatchFactoryCore.Scripts.State;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MatchFactoryCore.Scripts.Game
{
    [DefaultExecutionOrder(-100)]
    public class ControllerMatchFactory : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private ControllerItemFactory3D controllerItemFactory3D;

        public ControllerItemFactory3D ControllerItemFactory3D => controllerItemFactory3D;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private ControllerCollectionBar controllerCollectionBar;
        [SerializeField] private InfoItemsMatch3Factory infoItemsMatch3Factory;
        [SerializeField] private InfoLevelsMatch3Factory infoLevelsMatch3Factory;
        [SerializeField] private GameObject holder;

        //Test
        public string stateName;
        public float spawnInHighValue;
        public float maxX, maxZ;

        public event Action<Dictionary<ItemFactoryType, int>> OnLevelTargetChanged;
        public event Action<float> OnTimeLevelChanged;
        public float TimeLevel { get; private set; }

        private StateMachine<MatchFactoryState> _stateMachine;
        private readonly Dictionary<ItemFactoryType, int> _targetDictionary = new();
        private readonly List<Rigidbody> _itemsRigidbody = new List<Rigidbody>();
        private readonly Dictionary<int, ItemFactory> _dictItemFactory = new Dictionary<int, ItemFactory>();

        // Cache
        Vector3 _randomSpawnPoint;

        private void OnEnable()
        {
            controllerItemFactory3D.OnPointerReleased += CheckLevelTarget;
            controllerCollectionBar.OnItemMatched += RemoveItemFactory;
            controllerCollectionBar.OnInsertItemCompleted += CheckLose;
        }

        private void OnDisable()
        {
            controllerItemFactory3D.OnPointerReleased -= CheckLevelTarget;
            controllerCollectionBar.OnItemMatched -= RemoveItemFactory;
            controllerCollectionBar.OnInsertItemCompleted -= CheckLose;
        }

        private void Awake()
        {
            InitializeDictionary();
            InitializeState();
        }

        private void Start()
        {
            OnLevelTargetChanged?.Invoke(_targetDictionary);
        }

        private void Update()
        {
            _stateMachine.UpdateState();
        }

        #region Initialize

        void InitializeState()
        {
            _stateMachine = new StateMachine<MatchFactoryState>();
            _stateMachine.AddState(MatchFactoryState.Init, new InitState(this, _stateMachine));
            _stateMachine.AddState(MatchFactoryState.Playing, new PlayingState(this, _stateMachine));
            _stateMachine.AddState(MatchFactoryState.Pause, new PauseState(this, _stateMachine));
            _stateMachine.AddState(MatchFactoryState.Win, new WinState(this, _stateMachine));
            _stateMachine.AddState(MatchFactoryState.Lose, new LoseState(this, _stateMachine));
            _stateMachine.SetInitState(MatchFactoryState.Init);
        }

        void InitializeDictionary()
        {
            infoItemsMatch3Factory.SetCache();
        }

        public void InitializeLevel(int level, Action onReady)
        {
            var dataLevel = infoLevelsMatch3Factory.CacheDictInfoLevelsMatch3Factory[level];
            TimeLevel = dataLevel.TimeLevel;
            var levelTarget = dataLevel.DictLevelTarget;
            var otherObjInLevel = dataLevel.DictOtherObjectInLevel;

            _itemsRigidbody.Clear();

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

        [Obsolete]
        IEnumerator WaitForReady(Action onReady)
        {
            yield return null;
            while (true)
            {
                var isReady = true;
                foreach (var rigid in _itemsRigidbody)
                {
                    if (!rigid.IsSleeping())
                    {
                        isReady = false;
                        break;
                    }
                }

                if (isReady)
                {
                    Debug.Log("Ready");
                    onReady?.Invoke();
                    yield break;
                }

                yield return null;
            }
        }

        #endregion

        #region Spawn Helper

        void Spawn(ItemFactoryType itemFactoryType, int index)
        {
            infoItemsMatch3Factory.CacheDictInfoItemsMatch3Factory.TryGetValue(itemFactoryType, out var so);
            if (so == null)
            {
                Debug.LogError($"{itemFactoryType} not found");
                return;
            }

            var newItemFactory3D = Instantiate(so.prefab, GetRandomSpawnPoint(), Quaternion.identity);
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
            IItemFactory3D itemFactory3D = itemFactoryComp as IItemFactory3D;
            _itemsRigidbody.Add(itemFactory3D.ObjectRigidbody);
        }

        Vector3 GetRandomSpawnPoint()
        {
            var randX = Random.Range(-maxX, maxX);
            var randZ = Random.Range(-maxZ, maxZ);
            _randomSpawnPoint.x = randX;
            _randomSpawnPoint.y = spawnInHighValue;
            _randomSpawnPoint.z = randZ;
            return _randomSpawnPoint;
        }

        #endregion

        #region Event Actions

        private void CheckLevelTarget(int id)
        {
            _dictItemFactory.TryGetValue(id, out ItemFactory itemFactory);
            if (itemFactory == null) return;

            _targetDictionary.TryGetValue(itemFactory.ItemFactoryType, out int remainTarget);
            if (remainTarget > 0)
            {
                _targetDictionary[itemFactory.ItemFactoryType]--;
                if (IsWin())
                {
                    _stateMachine.ChangeState(MatchFactoryState.Win);
                }

                OnLevelTargetChanged?.Invoke(_targetDictionary);
            }

            Vector3 targetJumpPos = controllerCollectionBar.GetPositionTo3DJump(itemFactory.ItemFactoryType);
            targetJumpPos.y -= mainCamera.transform.position.y;
            Debug.Log(targetJumpPos);
            controllerCollectionBar.SpawnItemFactory2D(GetItemFactory2DById(id), id);
            itemFactory.JumpFromBoard(targetJumpPos, () =>
            {
                controllerCollectionBar.SetActiveItemChoose(id);
            });
        }

        private void RemoveItemFactory(List<int> idList)
        {
            for (int i = 0; i < idList.Count; i++)
            {
                Destroy(_dictItemFactory[idList[i]].gameObject);
                _dictItemFactory.Remove(idList[i]);
            }
        }

        bool IsWin()
        {
            bool isWin = true;
            foreach (var item in _targetDictionary)
            {
                if (item.Value != 0)
                {
                    isWin = false;
                    break;
                }
            }

            return isWin;
        }

        private void CheckLose(bool isFull)
        {
            if (isFull)
            {
                _stateMachine.ChangeState(MatchFactoryState.Lose);
            }
        }

        #endregion

        public void UpdateTimeLevel()
        {
            TimeLevel -= Time.deltaTime;
            if (TimeLevel <= 0) TimeLevel = 0;
            OnTimeLevelChanged?.Invoke(TimeLevel);
        }

        private IItemFactory2D GetItemFactory2DById(int id)
        {
            _dictItemFactory.TryGetValue(id, out var itemFactory);
            if (itemFactory == null) return null;
            return itemFactory as IItemFactory2D;
        }
    }
}
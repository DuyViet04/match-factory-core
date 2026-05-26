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
    public enum MatchFactoryState
    {
        Init,
        Playing,
        Pause,
        Win,
        Lose
    }

    [DefaultExecutionOrder(-100)]
    public class ControllerMatchFactory : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private ControllerItemFactory3D controllerItemFactory3D;

        public ControllerItemFactory3D ControllerItemFactory3D => controllerItemFactory3D;

        [SerializeField] private ControllerCollectionBar controllerCollectionBar;
        [SerializeField] private InfoItemsMatch3Factory infoItemsMatch3Factory;
        [SerializeField] private InfoLevelsMatch3Factory infoLevelsMatch3Factory;
        [SerializeField] private GameObject holder;
        [SerializeField] private List<GameObject> itemSlots = new List<GameObject>();

        //Test
        public float spawnInHighValue;
        public float maxX, maxZ;

        public static readonly int MaxSlot = 7;
        public List<GameObject> ItemSlots => itemSlots;
        public event Action<Dictionary<ItemFactoryType, int>> OnLevelTargetChanged;
        public event Action<float> OnTimeLevelChanged;
        public float TimeLevel { get; private set; }

        private StateMachine<MatchFactoryState> _stateMachine;
        private readonly Dictionary<ItemFactoryType, int> _targetDictionary = new();
        private readonly List<Rigidbody> _itemsRigidbody = new List<Rigidbody>();
        private readonly Dictionary<int, ItemFactory> _dictItemFactory = new Dictionary<int, ItemFactory>();

        // Cache
        Vector3 _randomSpawnPoint;
        int _id;

        private void OnEnable()
        {
            controllerItemFactory3D.OnPointerReleased += CheckLevelTarget;
            controllerItemFactory3D.OnJumpOnBoardComplete += SpawnItemFactory2D;
        }

        private void OnDisable()
        {
            controllerItemFactory3D.OnPointerReleased -= CheckLevelTarget;
            controllerItemFactory3D.OnJumpOnBoardComplete -= SpawnItemFactory2D;
        }

        private void Awake()
        {
            InitializeDictionary();
            InitializeState();
        }

        private void Update()
        {
            _stateMachine.UpdateState();
        }

        #region InitializeItemFactory3D

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
            OnLevelTargetChanged?.Invoke(_targetDictionary);
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

            StartCoroutine(WaitForReady(onReady));
        }

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

        #region Helper

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
            ItemFactory itemFactoryComp = newItemFactory3D.AddComponent<ItemFactory>();

            var outline = newItemFactory3D.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 5;
            outline.enabled = false;

            InitItemFactory3DContext initItemFactory3DContext = new InitItemFactory3DContext()
            {
                Id = (int)itemFactoryType + index,
                FactoryType = itemFactoryType,
                Prefab = newItemFactory3D,
                Sprite = so.sprite,
                Size = so.prefabSize,
                PrefabScale = so.prefabScale,
                PrefabBaseRotation = so.prefab.transform.rotation.eulerAngles,
                SpriteScaleOnBar = so.spriteScaleOnBar,
                SpriteScaleWhenChange = so.spriteScaleWhenChange
            };
            itemFactoryComp.Initialize(initItemFactory3DContext);
            _dictItemFactory.TryAdd(initItemFactory3DContext.Id, itemFactoryComp);
            IItemFactory3D itemFactory3D = itemFactoryComp as IItemFactory3D;
            _itemsRigidbody.Add(itemFactory3D.RigidbodyObject);
        }

        private void SpawnItemFactory2D(Vector3 spawnPoint)
        {
            controllerCollectionBar.SpawnItemFactory2D(GetItemFactory2DById(_id), spawnPoint,
                rectTransform => { _dictItemFactory[_id].SpriteTransform = rectTransform; });
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

        private void CheckLevelTarget(int id)
        {
            _id = id;
            _dictItemFactory.TryGetValue(id, out var itemFactory);
            if (itemFactory == null) return;
            _targetDictionary.TryGetValue(itemFactory.ItemFactoryType, out var remainTarget);
            if (remainTarget > 0)
            {
                _targetDictionary[itemFactory.ItemFactoryType]--;
                if (IsWin())
                {
                    _stateMachine.ChangeState(MatchFactoryState.Win);
                }
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
            return (IItemFactory2D)itemFactory;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game.State;
using MatchFactoryCore.Scripts.Input;
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
    public class MatchFactoryController : MonoBehaviour
    {
        [SerializeField] private MatchFactoryInput input;
        [Header("Data")] [SerializeField] private InfoItemsMatch3Factory infoItemsMatch3Factory;
        [SerializeField] private InfoLevelsMatch3Factory infoLevelsMatch3Factory;
        [SerializeField] private GameObject holder;
        [SerializeField] private List<GameObject> itemSlots = new List<GameObject>();

        //Test
        public float spawnInHighValue;
        public float maxX, maxZ;

        public static readonly int MaxSlot = 7;
        public MatchFactoryInput Input => input;
        public List<GameObject> ItemSlots => itemSlots;
        public event Action<int> OnClickTarget;
        public event Action<float> OnTimeLevelChanged;
        public float TimeLevel { get; private set; }

        private StateMachine<MatchFactoryState> _stateMachine;
        private readonly List<Rigidbody> _itemsRigidbody = new List<Rigidbody>();
        public Dictionary<ItemFactoryType, int> TargetDictionary => _targetDictionary;
        private readonly Dictionary<ItemFactoryType, int> _targetDictionary = new();

        // Cache
        private Vector3 _randomSpawnPoint;

        private void Awake()
        {
            InitializeDictionary();
            InitializeState();
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

            var newItemFactory = new GameObject("ItemFactory");
            newItemFactory.transform.parent = holder.transform;
            newItemFactory.transform.SetPositionAndRotation(GetRandomSpawnPoint(), Quaternion.identity);
            ItemFactory itemFactoryComp = newItemFactory.AddComponent<ItemFactory>();
            var newObject3D = Instantiate(so.prefab, so.prefab.transform.position, so.prefab.transform.rotation);
            var newObject2D = Instantiate(so.sprite, so.sprite.transform.position, so.sprite.transform.rotation);
            newObject3D.transform.parent = newItemFactory.transform;
            newObject2D.transform.parent = newItemFactory.transform;
            newObject3D.transform.localPosition = Vector3.zero;
            newObject2D.transform.localPosition = Vector3.zero;
            newObject2D.SetActive(false);

            var outline = newObject3D.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 5;
            outline.enabled = false;

            InitContext initContext = new InitContext()
            {
                Id = (int)itemFactoryType + index,
                FactoryType = itemFactoryType,
                Prefab = newObject3D,
                Sprite = newObject2D,
                Size = so.prefabSize,
                PrefabScale = so.prefabScale,
                PrefabBaseRotation = so.prefab.transform.rotation.eulerAngles,
                SpriteScaleOnBar = so.spriteScaleOnBar,
                SpriteScaleWhenChange = so.spriteScaleWhenChange
            };
            // Debug.Log(
            //     $"{initContext.Id} {initContext.PrefabScale} {initContext.SpriteScaleOnBar} {initContext.SpriteScaleWhenChange}");
            itemFactoryComp.Initialize(initContext);

            IItemFactory3D itemFactory3D = itemFactoryComp as IItemFactory3D;
            _itemsRigidbody.Add(itemFactory3D.RigidbodyObject);
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

        public void ClickTarget(int currentTarget)
        {
            OnClickTarget?.Invoke(currentTarget);
        }

        #endregion

        public void ActiveInput(bool enable)
        {
            input.gameObject.SetActive(enable);
        }

        public void UpdateTimeLevel()
        {
            TimeLevel -= Time.deltaTime;
            if (TimeLevel <= 0) TimeLevel = 0;
            OnTimeLevelChanged?.Invoke(TimeLevel);
        }
    }
}
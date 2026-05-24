using System;
using System.Collections;
using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game.State;
using MatchFactoryCore.Scripts.Input;
using MatchFactoryCore.Scripts.Item;
using MatchFactoryCore.Scripts.State;
using UnityEngine;

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

    public class MatchFactoryController : MonoBehaviour
    {
        [SerializeField] private MatchFactoryInput input;
        [Header("Data")] [SerializeField] private ItemDictionarySo itemDictionarySo;
        [SerializeField] private LevelDataSo levelDataSo;
        [SerializeField] private GameObject holder;
        [SerializeField] private List<GameObject> itemSlots = new List<GameObject>();

        //Test
        public Vector3 spawnPoint;

        public static readonly int MaxSlot = 7;
        private StateMachine<MatchFactoryState> _stateMachine;
        private readonly List<Rigidbody> _itemRigids = new List<Rigidbody>();

        private readonly Dictionary<ItemType, ItemFactoryDataSo> _itemDataDictionary =
            new Dictionary<ItemType, ItemFactoryDataSo>();

        public MatchFactoryInput Input => input;
        public List<GameObject> ItemSlots => itemSlots;

        // Cache
        private List<ObjectInLevel> _levelTargets;
        private List<ObjectInLevel> _otherObjects;

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
            foreach (var item in itemDictionarySo.items)
            {
                _itemDataDictionary.TryAdd(item.itemType, item.itemData);
            }
        }

        public void InitializeLevel(Action onReady)
        {
            _levelTargets = new List<ObjectInLevel>(levelDataSo.levelTarget);
            _otherObjects = new List<ObjectInLevel>(levelDataSo.otherObjects);

            // Spawn item cần thu thập
            foreach (var item in _levelTargets)
            {
                for (int i = 0; i < item.number; i++)
                {
                    Spawn(item.itemType, i);
                }
            }

            // Spawn item khác
            foreach (var item in _otherObjects)
            {
                for (int i = 0; i < item.number; i++)
                {
                    Spawn(item.itemType, i);
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
                foreach (var rigid in _itemRigids)
                {
                    if (!rigid.IsSleeping()) isReady = false;
                    break;
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

        void Spawn(ItemType itemType, int index)
        {
            _itemRigids.Clear();
            _itemDataDictionary.TryGetValue(itemType, out var so);
            if (so == null)
            {
                Debug.LogError($"{itemType} not found");
                return;
            }

            var newObj = Instantiate(so.prefab, spawnPoint, so.prefab.transform.rotation);
            newObj.transform.parent = holder.transform;
            Rigidbody itemRigid = newObj.AddComponent<Rigidbody>();
            ItemFactory itemFactory = newObj.AddComponent<ItemFactory>();
            itemFactory.Initialize((int)itemType + index, itemType, newObj, so.sprite, so.size);

            _itemRigids.Add(itemRigid);
        }

        #endregion

        public void ActiveInput(bool enable)
        {
            input.gameObject.SetActive(enable);
        }
    }
}
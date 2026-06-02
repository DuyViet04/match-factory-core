using System;
using System.Collections.Generic;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game.State;
using MatchFactoryCore.Scripts.Item;
using MatchFactoryCore.Scripts.State;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game
{
    [DefaultExecutionOrder(-100)]
    public class ControllerMatchFactory : MonoBehaviour
    {
        private static ControllerMatchFactory _instance;

        public static ControllerMatchFactory Ins
        {
            get
            {
                if (_instance == null) Debug.LogError("ControllerMatchFactory not instance");
                return _instance;
            }
        }

        [Header("References")] [SerializeField]
        private ControllerItemFactory3D controllerItemFactory3D;

        public ControllerItemFactory3D ControllerItemFactory3D => controllerItemFactory3D;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private ControllerCollectionBar controllerCollectionBar;
        [SerializeField] private ControllerItemBooster controllerItemBooster;

        //Test
        public string stateName;

        public event Action<Dictionary<ItemFactoryType, int>> OnLevelTargetChanged;
        public event Action<float> OnTimeLevelChanged;
        public float TimeLevel { get; private set; }

        private Dictionary<ItemFactoryType, int> _targetDictionary;
        private Dictionary<ItemFactoryType, int> _otherItemDictionary;
        private Dictionary<int, ItemFactory> _dictItemFactory;
        private StateMachine<MatchFactoryState> _stateMachine;


        private void OnEnable()
        {
            controllerItemFactory3D.OnPointerReleased += CheckLevelTarget;
            controllerItemFactory3D.OnItemActionHourglassUse += UpdateTime;
            controllerCollectionBar.OnItemMatched += RemoveItemFactory;
            controllerCollectionBar.OnInsertItemCompleted += CheckLose;
            controllerItemBooster.OnVacuumBoosterUse += OnVacuumBoosterUse;
        }

        private void OnDisable()
        {
            controllerItemFactory3D.OnPointerReleased -= CheckLevelTarget;
            controllerItemFactory3D.OnItemActionHourglassUse -= UpdateTime;
            controllerCollectionBar.OnItemMatched -= RemoveItemFactory;
            controllerCollectionBar.OnInsertItemCompleted -= CheckLose;
            controllerItemBooster.OnVacuumBoosterUse -= OnVacuumBoosterUse;

        }

        private void OnVacuumBoosterUse(List<IItem3D> list3D, List<ItemFactory2D> list2D, Vector3 position)
        {
            if (list2D != null && list2D.Count > 0)
            {
                controllerCollectionBar.MoveToVacuum(list2D, position);
            }

            if (list3D != null && list3D.Count > 0)
            {
                controllerItemFactory3D.MoveToVacuum(list3D, position);
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Debug.LogError($"{GetType().Name} is already instantiated");
            }

            InitializeState();
            _targetDictionary = controllerItemFactory3D.GetTargetDictionary();
            _dictItemFactory = controllerItemFactory3D.GetDictItemFactory();
            _otherItemDictionary = controllerItemFactory3D.GetOtherItemDictionary();
        }

        private void Start()
        {
            OnLevelTargetChanged?.Invoke(controllerItemFactory3D.GetTargetDictionary());
        }

        private void Update()
        {
            _stateMachine.UpdateState();
        }

        #region Initialize

        private void InitializeState()
        {
            _stateMachine = new StateMachine<MatchFactoryState>();
            _stateMachine.AddState(MatchFactoryState.Init, new InitState(this, _stateMachine));
            _stateMachine.AddState(MatchFactoryState.Playing, new PlayingState(this, _stateMachine));
            _stateMachine.AddState(MatchFactoryState.Pause, new PauseState(this, _stateMachine));
            _stateMachine.AddState(MatchFactoryState.Win, new WinState(this, _stateMachine));
            _stateMachine.AddState(MatchFactoryState.Lose, new LoseState(this, _stateMachine));
            _stateMachine.SetInitState(MatchFactoryState.Init);
        }

        public void InitializeLevel(int level, Action onReady)
        {
            DataLevelMatchFactory dataLevel = controllerItemFactory3D.GetDataLevel(level);
            TimeLevel = dataLevel.TimeLevel;
            controllerItemFactory3D.SpawnAllItem(level, onReady);
        }

        #endregion

        #region Event Actions

        public MatchFactoryState GetCurrentState()
        {
            return _stateMachine.CurrentStateKey;
        }

        public void PushOnLevelTargetChangeEvent()
        {
            OnLevelTargetChanged?.Invoke(controllerItemFactory3D.GetTargetDictionary());
            if (IsWin())
            {
                _stateMachine.ChangeState(MatchFactoryState.Win);
            }
        }

        private void CheckLevelTarget(int id)
        {
            // Todo: chuyen ve func trong 3d
            _dictItemFactory.TryGetValue(id, out ItemFactory itemFactory);
            if (itemFactory == null) return;

            bool isTarget = _targetDictionary.TryGetValue(itemFactory.ItemFactoryType, out int remainTarget);
            bool isOther = _otherItemDictionary.TryGetValue(itemFactory.ItemFactoryType, out int remainOther);
            if (isTarget && remainTarget > 0)
            {
                _targetDictionary[itemFactory.ItemFactoryType]--;
                if (IsWin())
                {
                    _stateMachine.ChangeState(MatchFactoryState.Win);
                }

                OnLevelTargetChanged?.Invoke(_targetDictionary);
            }

            if (isOther && remainOther > 0)
            {
                _otherItemDictionary[itemFactory.ItemFactoryType]--;
                if (_otherItemDictionary[itemFactory.ItemFactoryType] <= 0)
                {
                    _otherItemDictionary.Remove(itemFactory.ItemFactoryType);
                }
            }

            Vector3 targetJumpPos = controllerCollectionBar.GetPositionTo3DJump(itemFactory.ItemFactoryType);
            targetJumpPos.y = 0;
            controllerCollectionBar.SpawnItemFactory2D(GetItemFactory2DById(id), id);
            itemFactory.ActionBehaviour(targetJumpPos, () => { controllerCollectionBar.SetActiveItemChoose(id); });
        }

        private void UpdateTime(int timeValue)
        {
            TimeLevel += timeValue;
            OnTimeLevelChanged?.Invoke(TimeLevel);
        }

        private void RemoveItemFactory(List<int> idList)
        {
            controllerItemFactory3D.RemoveItemFactory(idList);
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

        #region Gets Sets

        public Dictionary<int, ItemFactory> GetDictItemFactory()
        {
            return controllerItemFactory3D.GetDictItemFactory();
        }

        public Dictionary<ItemFactoryType, int> GetTargetDictionary()
        {
            return controllerItemFactory3D.GetTargetDictionary();
        }

        public bool CheckTargetItemById(int id)
        {
            return controllerItemFactory3D.CheckIsTargetItemById(id);
        }

        public List<IItem3D> GetRandomItemByType(ItemFactoryType type, int count)
        {
            return controllerItemFactory3D.GetRandomItemByType(type, count);
        }

        public List<ItemFactory> GetRandomTargetListItem()
        {
            return controllerItemFactory3D.GetRandomTargetListItem();
        }

        public List<ItemFactory2D> GetLastTargetItem2DOnBar()
        {
            return controllerCollectionBar.GetLastTargetItem2DOnBar();
        }

        public List<ItemFactory2D> GetAllItemTargetOnBarByType(ItemFactoryType type)
        {
            return controllerCollectionBar.GetAllItemTargetOnBarByType(type);
        }

        public List<ItemFactory2D> GetItemFactory2DList()
        {
            return controllerCollectionBar.GetItemFactory2DList();
        }

        public void PlaySequenceAfterUseHutBui()
        {
            controllerCollectionBar.PlaySequenceAfterUseHutBui();
        }

        public List<IItem3D> GetListItemRandomByBooster(int count)
        {
            return controllerItemFactory3D.GetListItemRandomByBooster(count);
        }
        
        #endregion
    }
}
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

        private StateMachine<MatchFactoryState> _stateMachine;


        private void OnEnable()
        {
            controllerItemFactory3D.OnPointerReleased += HandleSpawnItem2D;
            controllerItemFactory3D.OnItemActionHourglassUse += UpdateTime;
            controllerItemFactory3D.OnRemainTargetChanged += UpdateLevelTarget;
            controllerCollectionBar.OnItemMatched += RemoveItemFactory;
            controllerCollectionBar.OnInsertItemCompleted += CheckLose;
            controllerItemBooster.OnVacuumBoosterUsed += HandleBoosterVacuumUsed;
            controllerItemBooster.OnSpringBoosterUsed += HandleBoosterSpringUsed;
        }

        private void OnDisable()
        {
            controllerItemFactory3D.OnPointerReleased -= HandleSpawnItem2D;
            controllerItemFactory3D.OnItemActionHourglassUse -= UpdateTime;
            controllerItemFactory3D.OnRemainTargetChanged -= UpdateLevelTarget;
            controllerCollectionBar.OnItemMatched -= RemoveItemFactory;
            controllerCollectionBar.OnInsertItemCompleted -= CheckLose;
            controllerItemBooster.OnVacuumBoosterUsed -= HandleBoosterVacuumUsed;
            controllerItemBooster.OnSpringBoosterUsed -= HandleBoosterSpringUsed;
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

        private void HandleSpawnItem2D(int id)
        {
            controllerItemFactory3D.GetDictItemFactory().TryGetValue(id, out ItemFactory itemFactory);
            IItemFactory2D item2D = itemFactory;
            IItem3D item3D = itemFactory;
            if (item2D != null)
            {
                Vector3 targetPos = controllerCollectionBar.GetPositionTo3DJump(item2D.ItemFactoryType);
                targetPos.y = 0;
                controllerCollectionBar.SpawnItemFactory2D(item2D, id);
                item3D.ActionBehaviour(targetPos, () => { controllerCollectionBar.SetActiveItemChoose(id); });
            }
        }

        private void UpdateLevelTarget(Dictionary<ItemFactoryType, int> targetDict)
        {
            OnLevelTargetChanged?.Invoke(targetDict);
            bool isWin = CheckWin(targetDict);
            if (isWin)
            {
                _stateMachine.ChangeState(MatchFactoryState.Win);
            }
        }

        private void HandleBoosterVacuumUsed(List<IItem3D> list3D, List<ItemFactory2D> list2D, Vector3 position)
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

        private void HandleBoosterSpringUsed(int id)
        {
            ItemFactory2D itemFactory2D = controllerCollectionBar.GetItemFactory2DById(id);
            Vector3 startPos = mainCamera.ScreenToWorldPoint(itemFactory2D.RectTransform.position);
            startPos.y = mainCamera.transform.position.y / 2f;

            controllerItemFactory3D.HandleBoosterSpringUsed(id, startPos);
            controllerCollectionBar.HandleBoosterSpringUsed(id);
        }

        private void RemoveItemFactory(List<int> idList)
        {
            controllerItemFactory3D.RemoveItemFactory(idList);
        }

        bool CheckWin(Dictionary<ItemFactoryType, int> targetDict)
        {
            bool isWin = true;
            foreach (var itemTemp in targetDict)
            {
                if (itemTemp.Value != 0)
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

        public void UpdateTimeLevel()
        {
            TimeLevel -= Time.deltaTime;
            if (TimeLevel <= 0) TimeLevel = 0;
            OnTimeLevelChanged?.Invoke(TimeLevel);
        }

        private void UpdateTime(int timeValue)
        {
            TimeLevel += timeValue;
            OnTimeLevelChanged?.Invoke(TimeLevel);
        }

        #endregion

        #region Gets Sets

        public bool CheckTargetItemById(int id)
        {
            return controllerItemFactory3D.CheckIsTargetItemById(id);
        }

        public List<IItem3D> GetRandomItemByType(ItemFactoryType type, int count)
        {
            return controllerItemFactory3D.GetRandomItemsByType(type, count);
        }

        public List<ItemFactory2D> GetLastTargetItem2DOnBar()
        {
            return controllerCollectionBar.GetLastTargetItem2DOnBar();
        }

        public List<IItem3D> GetListItemRandomByBooster(int count)
        {
            return controllerItemFactory3D.GetListItemRandomByBooster(count);
        }

        public ItemFactory2D GetLastItem2DOnBar()
        {
            return controllerCollectionBar.GetLastItem2DOnBar();
        }

        public MatchFactoryState GetCurrentState()
        {
            return _stateMachine.CurrentStateKey;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using MatchFactoryCore.Scripts.UI;
using MatchFactoryCore.Scripts.Data;
using MatchFactoryCore.Scripts.Game.State;
using MatchFactoryCore.Scripts.Item;
using MatchFactoryCore.Scripts.State;
using UnityEngine;

// TODO: Lose Check
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

        public static bool HasInstance => _instance != null;

        [Header("References")]
        [SerializeField] private ControllerItemFactory3D controllerItemFactory3D;

        public ControllerItemFactory3D ControllerItemFactory3D => controllerItemFactory3D;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private ControllerCollectionBar controllerCollectionBar;
        [SerializeField] private ControllerItemBooster controllerItemBooster;
        [SerializeField] private TimeLevelUI timeLevelUI;

        //Test
        public string stateName;

        public event Action<Dictionary<ItemFactoryType, int>> OnLevelTargetChanged;
        public event Action<float> OnTimeLevelChanged;
        public event Action<float> OnFreezeTimeChanged;
        public Vector3 FireworkAnchorPos = new Vector3(0, 7, 3.5f);
        public float TimeLevel { get; private set; }
        public bool IsFullBar { get; private set; }

        private StateMachine<MatchFactoryState> _stateMachine;
        private bool _isFreezeTime;
        private float _timeFreeze;

        private void OnEnable()
        {
            controllerItemFactory3D.OnPointerReleased += HandleSpawnItem2D;
            controllerItemFactory3D.OnItemActionHourglassUsed += UpdateTime;
            controllerItemFactory3D.OnRemainTargetChanged += UpdateLevelTarget;
            controllerCollectionBar.OnItemMatched += RemoveItemFactory;
            controllerCollectionBar.OnInsertItemCompleted += CheckLose;
            controllerCollectionBar.OnMatchItemStarted += HandleRaycast;
            controllerItemBooster.OnVacuumBoosterUsed += HandleWhenBoosterVacuumUsed;
            controllerItemBooster.OnSpringBoosterUsed += HandleWhenBoosterSpringUsed;
            controllerItemBooster.OnFanBoosterUsed += HandleWhenBoosterFanUsed;
            controllerItemBooster.OnFreezeGunBoosterUsed += HandleWhenBoosterFreezeGunUsed;
        }

        private void OnDisable()
        {
            controllerItemFactory3D.OnPointerReleased -= HandleSpawnItem2D;
            controllerItemFactory3D.OnItemActionHourglassUsed -= UpdateTime;
            controllerItemFactory3D.OnRemainTargetChanged -= UpdateLevelTarget;
            controllerCollectionBar.OnItemMatched -= RemoveItemFactory;
            controllerCollectionBar.OnInsertItemCompleted -= CheckLose;
            controllerCollectionBar.OnMatchItemStarted -= HandleRaycast;
            controllerItemBooster.OnVacuumBoosterUsed -= HandleWhenBoosterVacuumUsed;
            controllerItemBooster.OnSpringBoosterUsed -= HandleWhenBoosterSpringUsed;
            controllerItemBooster.OnFanBoosterUsed -= HandleWhenBoosterFanUsed;
            controllerItemBooster.OnFreezeGunBoosterUsed -= HandleWhenBoosterFreezeGunUsed;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
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
        }

        private void Start()
        {
            OnLevelTargetChanged?.Invoke(controllerItemFactory3D.GetTargetDictionary());
            IsFullBar = false;
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

        private void HandleRaycast(bool enable)
        {
            IsFullBar = enable;
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

        private void HandleWhenBoosterVacuumUsed(List<IItem3D> list3D, List<ItemFactory2D> list2D, Vector3 position)
        {
            float delay = 0.25f;
            if (list2D != null && list2D.Count > 0)
            {
                controllerCollectionBar.MoveToVacuum(list2D, position, delay,
                    () => { controllerCollectionBar.PlaySequenceAfterUseVacuum(); });
                delay += 0.25f * list2D.Count;
            }

            if (list3D != null && list3D.Count > 0)
            {
                controllerItemFactory3D.MoveToVacuum(list3D, position, delay);
            }
        }

        private void HandleWhenBoosterSpringUsed(int id)
        {
            ItemFactory2D itemFactory2D = controllerCollectionBar.GetItemFactory2DById(id);
            Vector3 startPos = mainCamera.ScreenToWorldPoint(itemFactory2D.RectTransform.position);
            startPos.y = mainCamera.transform.position.y / 2f;

            controllerItemFactory3D.JumpToBoard(id, startPos);
            controllerCollectionBar.DisableItem2D(id);
        }

        private void HandleWhenBoosterFanUsed()
        {
            controllerItemFactory3D.BlowByFanBooster();
        }

        private void HandleWhenBoosterFreezeGunUsed(int timeFreeze)
        {
            _isFreezeTime = true;
            _timeFreeze += timeFreeze;
            OnFreezeTimeChanged?.Invoke(_timeFreeze);
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
            if (_isFreezeTime) return;
            TimeLevel -= Time.deltaTime;
            if (TimeLevel <= 0) TimeLevel = 0;
            OnTimeLevelChanged?.Invoke(TimeLevel);
        }

        public void UpdateFreezeTime()
        {
            if (!_isFreezeTime) return;
            _timeFreeze -= Time.deltaTime;
            if (_timeFreeze <= 0)
            {
                _isFreezeTime = false;
                _timeFreeze = 0;
            }

            OnFreezeTimeChanged?.Invoke(_timeFreeze);
        }

        private void UpdateTime(float timeValue)
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

        public List<IItem3D> GetListItemRandomByItemAction(int count)
        {
            return controllerItemFactory3D.GetListItemRandomByItemAction(count);
        }

        public ItemFactory2D GetLastItem2DOnBar()
        {
            return controllerCollectionBar.GetLastItem2DOnBar();
        }

        public MatchFactoryState GetCurrentState()
        {
            return _stateMachine.CurrentStateKey;
        }

        public Vector3 GetTimeUIPosition()
        {
            Vector3 timeUIPos = timeLevelUI.GetTimeUIPosition();
            Vector3 timeUIWorldPos = mainCamera.ScreenToWorldPoint(timeUIPos);
            timeUIWorldPos.y -= 1;
            return timeUIWorldPos;
        }

        #endregion
    }
}
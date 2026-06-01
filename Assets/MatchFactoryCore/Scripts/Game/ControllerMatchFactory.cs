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
        [Header("References")] [SerializeField]
        private ControllerItemFactory3D controllerItemFactory3D;

        public ControllerItemFactory3D ControllerItemFactory3D => controllerItemFactory3D;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private ControllerCollectionBar controllerCollectionBar;

        //Test
        public string stateName;

        public event Action<Dictionary<ItemFactoryType, int>> OnLevelTargetChanged;
        public event Action<float> OnTimeLevelChanged;
        public float TimeLevel { get; private set; }

        private Dictionary<ItemFactoryType, int> _targetDictionary;
        private Dictionary<int, ItemFactory> _dictItemFactory;
        private StateMachine<MatchFactoryState> _stateMachine;


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
            InitializeState();
            _targetDictionary = controllerItemFactory3D.GetTargetDictionary();
            _dictItemFactory = controllerItemFactory3D.GetDictItemFactory();
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
            controllerCollectionBar.SpawnItemFactory2D(GetItemFactory2DById(id), id);
            itemFactory.ActionBehaviour(targetJumpPos, () => { controllerCollectionBar.SetActiveItemChoose(id); });
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
    }
}
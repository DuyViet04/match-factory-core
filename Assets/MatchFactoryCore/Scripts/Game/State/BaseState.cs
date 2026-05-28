using MatchFactoryCore.Scripts.State;

namespace MatchFactoryCore.Scripts.Game.State
{
    public enum MatchFactoryState
    {
        Init,
        Playing,
        Pause,
        Win,
        Lose
    }

    public abstract class BaseState : IState
    {
        protected readonly ControllerMatchFactory Controller;
        protected readonly StateMachine<MatchFactoryState> StateMachine;

        protected BaseState(ControllerMatchFactory controller, StateMachine<MatchFactoryState> stateMachine)
        {
            Controller = controller;
            StateMachine = stateMachine;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
        }
    }
}
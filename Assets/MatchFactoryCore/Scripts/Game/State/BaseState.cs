using MatchFactoryCore.Scripts.State;

namespace MatchFactoryCore.Scripts.Game.State
{
    public abstract class BaseState : IState
    {
        protected readonly MatchFactoryController Controller;
        protected readonly StateMachine<MatchFactoryState> StateMachine;

        protected BaseState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine)
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
using MatchFactoryCore.Scripts.State;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class PauseState : BaseState
    {
        public PauseState(ControllerMatchFactory controller, StateMachine<MatchFactoryState> stateMachine) : base(
            controller, stateMachine)
        {
        }
    }
}
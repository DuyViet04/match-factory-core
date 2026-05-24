using MatchFactoryCore.Scripts.State;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class PauseState : BaseState
    {
        public PauseState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(controller, stateMachine)
        {
        }
    }
}
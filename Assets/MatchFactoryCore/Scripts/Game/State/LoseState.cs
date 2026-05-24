using MatchFactoryCore.Scripts.State;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class LoseState : BaseState
    {
        public LoseState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(controller, stateMachine)
        {
        }
    }
}
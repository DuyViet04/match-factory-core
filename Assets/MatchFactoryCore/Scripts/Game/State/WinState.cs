using MatchFactoryCore.Scripts.State;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class WinState : BaseState
    {
        public WinState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(controller, stateMachine)
        {
        }
    }
}
using MatchFactoryCore.Scripts.State;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class InitState : BaseState
    {
        public InitState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(
            controller, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("Enter Init State");
            Controller.ActiveInput(false);
            Controller.InitializeLevel(() => StateMachine.ChangeState(MatchFactoryState.Playing));
        }
    }
}
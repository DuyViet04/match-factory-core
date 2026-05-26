using MatchFactoryCore.Scripts.State;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class InitState : BaseState
    {
        public InitState(ControllerMatchFactory controller, StateMachine<MatchFactoryState> stateMachine) : base(
            controller, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("Enter Init State");
            Controller.InitializeLevel(1, () => StateMachine.ChangeState(MatchFactoryState.Playing));
        }
    }
}
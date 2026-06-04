using MatchFactoryCore.Scripts.State;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class PlayingState : BaseState
    {
        public PlayingState(ControllerMatchFactory controller, StateMachine<MatchFactoryState> stateMachine) : base(
            controller, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("Enter Playing State");
            Controller.stateName = "Play";
        }

        public override void OnUpdate()
        {
            Controller.UpdateTimeLevel();
            Controller.UpdateFreezeTime();

            if (Controller.TimeLevel <= 0)
            {
                StateMachine.ChangeState(MatchFactoryState.Lose);
                return;
            }

            if (!Controller.IsFullBar)
                Controller.ControllerItemFactory3D.UpdateRaycast();
        }
    }
}
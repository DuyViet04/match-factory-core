using MatchFactoryCore.Scripts.State;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class WinState : BaseState
    {
        public WinState(ControllerMatchFactory controller, StateMachine<MatchFactoryState> stateMachine) : base(controller, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.LogWarning("OnEnter WinState");
        }
    }
}
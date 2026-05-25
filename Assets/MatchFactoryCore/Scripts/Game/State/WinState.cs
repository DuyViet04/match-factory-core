using MatchFactoryCore.Scripts.State;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class WinState : BaseState
    {
        public WinState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(controller, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.LogWarning("OnEnter WinState");
        }
    }
}
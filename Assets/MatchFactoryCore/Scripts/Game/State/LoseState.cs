using MatchFactoryCore.Scripts.State;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game.State
{
    public class LoseState : BaseState
    {
        public LoseState(MatchFactoryController controller, StateMachine<MatchFactoryState> stateMachine) : base(
            controller, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Debug.LogWarning("OnEnter LoseState");
        }
    }
}
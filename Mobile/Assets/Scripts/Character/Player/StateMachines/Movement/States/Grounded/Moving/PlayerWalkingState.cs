using UnityEngine.InputSystem;

namespace MovementSystemGI
{
    public class PlayerWalkingState:PlayerGroundedState
    {
        public PlayerWalkingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        #region IState Methods

        public override void Enter()
        {
            base.Enter();
            speedModifier = 0.225f;
        }

        #endregion

    


        #region Input Methods
        protected override void OnWalkToggleStarted(InputAction.CallbackContext context)
        {
            base.OnWalkToggleStarted(context);
            stateMachine.ChangeState(stateMachine.RunningState);
        }
        
        
        #endregion
        
    }
}
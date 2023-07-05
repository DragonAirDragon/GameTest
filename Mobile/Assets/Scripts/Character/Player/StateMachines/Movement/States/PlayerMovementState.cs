using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

namespace MovementSystemGI
{
    public class PlayerMovementState:IState
    {
        protected PlayerMovementStateMachine stateMachine;
        protected Vector2 movementInput;
        protected float baseSpeed = 5f;
        protected float speedModifier = 1f;

        protected Vector3 currentTargetRotation;
        protected Vector3 timeToReachTargetRotation;
        protected Vector3 dampedTargetRotationCurrentVelocity;
        protected Vector3 dampedTargetRotationPassedTime;

        protected bool shouldWalk;
        public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
        {
            stateMachine = playerMovementStateMachine;
            InitializeData();
        }

        private void InitializeData()
        {
            timeToReachTargetRotation.y = 0.14f;
        }

        #region IState Methods
        public virtual void Enter()
        {
            Debug.Log("State: "+ GetType().Name);
            AddInputActionsCallbacks();
        }
        public virtual void Exit()
        {
            RemoveInputActionsCallbacks();
        }



        public virtual void HandleInput()
        {
            ReadMovementInput();
        }

        

        public virtual void Update()
        {
            
        }

        public virtual void PhysicsUpdate()
        {
            Move();
        }
        
        #endregion
        #region Main Methods
        
        private void ReadMovementInput()
        {
            movementInput = stateMachine.Player.Input.PlayerActions.Movement.ReadValue<Vector2>();
        }
        private void Move()
        {
            if (movementInput == Vector2.zero||speedModifier==0f)
            {
                return;
            }

            Vector3 movementDirection = GetMovementInputDirection();

            float targetRotationYAngle = Rotate(movementDirection);

            Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle);
            
            float movementSpeed = GetMovementSpeed();
            Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();
            stateMachine.Player.Rigidbody.AddForce(targetRotationDirection*movementSpeed-currentPlayerHorizontalVelocity,ForceMode.VelocityChange);
        }

        

        private float Rotate(Vector3 directrion)
        {
            var directionAngle = UpdateTargetRotation(directrion);
            RotateTowardsTargetRotation();
            return directionAngle;
        }

        


        private float GetDirectionAngle(Vector3 directrion)
        {
            float directionAngle = Mathf.Atan2(directrion.x, directrion.z) * Mathf.Rad2Deg;
            if (directionAngle < 0f)
            {
                directionAngle += 360f;
            }

            return directionAngle;
        }
        
        private float AddCameraRotationAngle(float angle)
        {
            angle += stateMachine.Player.MainCameraTransform.eulerAngles.y;
            if (angle > 360f)
            {
                angle -= 360f;
            }

            return angle;
        }
        private void UpdateTargetRotationData(float targetAngle)
        {
            currentTargetRotation.y = targetAngle;
            dampedTargetRotationPassedTime.y = 0f;
        }
        #endregion
        #region Reusable Methods
        protected Vector3 GetMovementInputDirection()
        {
            return new Vector3(movementInput.x, 0f, movementInput.y);
        }
        protected float GetMovementSpeed()
        {
            return baseSpeed * speedModifier;
        }
        protected Vector3 GetPlayerHorizontalVelocity()
        {
            Vector3 playerHorizontalVelocity = stateMachine.Player.Rigidbody.velocity;
            playerHorizontalVelocity.y = 0f;
            return playerHorizontalVelocity;
        }
        protected void RotateTowardsTargetRotation()
        {
            float currentYAngle = stateMachine.Player.Rigidbody.rotation.eulerAngles.y;
            if (currentYAngle == currentTargetRotation.y)
            {
                return;
            }

            float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, currentTargetRotation.y,ref dampedTargetRotationCurrentVelocity.y,timeToReachTargetRotation.y-dampedTargetRotationPassedTime.y);
            dampedTargetRotationPassedTime.y += Time.deltaTime;
            Quaternion targetRotation = Quaternion.Euler(0f,smoothedYAngle,0f);
            stateMachine.Player.Rigidbody.MoveRotation(targetRotation);
        }
        protected float UpdateTargetRotation(Vector3 directrion,bool shouldConsiderCameraRotation =true)
        {
            float directionAngle = GetDirectionAngle(directrion);
            if (shouldConsiderCameraRotation)
            {
                directionAngle = AddCameraRotationAngle(directionAngle);
            }
            if (directionAngle != currentTargetRotation.y)
            {
                UpdateTargetRotationData(directionAngle);
            }

            return directionAngle;
        }
        protected Vector3 GetTargetRotationDirection(float targetAngle)
        {
            return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        protected void ResetVelocity()
        {
            stateMachine.Player.Rigidbody.velocity = Vector3.zero;
        }
        protected virtual void AddInputActionsCallbacks()
        {
            stateMachine.Player.Input.PlayerActions.WalkToggle.started += OnWalkToggleStarted;
        }
        protected virtual void RemoveInputActionsCallbacks()
        {
            stateMachine.Player.Input.PlayerActions.WalkToggle.started -= OnWalkToggleStarted;
        }
        #endregion

        #region Input Methods

        protected virtual void OnWalkToggleStarted(InputAction.CallbackContext context)
        {
            shouldWalk = !shouldWalk;
        }

        #endregion
    }
}
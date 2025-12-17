using UnityEngine;
using System;
/// <summary>
    /// Handles player movement behavior including walking, running, and rotation
    /// </summary>
    public class MovingState : BaseState
    {
        #region Constants
        private const float MOVING_THRESHOLD = 0.2f;
        private const float ROTATION_THRESHOLD = 0.1f;
        private const float WALK_SPEED = 3f;
        private const float RUN_SPEED = 6f;
        private const float ROTATION_SPEED = 6f;
        #endregion

        #region Fields
        private readonly PlayerComponents _playerComponents;
        private readonly IInputManager _inputManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new MovingState
        /// </summary>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="playerComponents">Player components reference</param>
        /// <param name="parentState">Optional parent state</param>
        public MovingState(HierarchicalStateMachine stateMachine, PlayerComponents playerComponents, IState parentState = null) 
            : base(stateMachine, parentState)
        {
            _playerComponents = playerComponents ?? throw new ArgumentNullException(nameof(playerComponents));
            
            try
            {
                _inputManager = DependencyInjectionManager.Instance?.Container?.Resolve<IInputManager>();
                if (_inputManager == null)
                {
                    Debug.LogError("[MovingState] Failed to resolve IInputManager");
                    return;
                }

                var updateMovementAnimationActivity = DependencyInjectionManager.Instance?.Container?.Resolve<UpdateMovementAnimationActivity>();
                if (updateMovementAnimationActivity != null)
                {
                    AddActivity(updateMovementAnimationActivity);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MovingState] Constructor failed: {e.Message}");
            }
        }
        #endregion

        #region Lifecycle Overrides
        /// <summary>
        /// Handles physics updates for movement and rotation
        /// </summary>
        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            
            if (_playerComponents?.CharacterActor == null || _inputManager == null)
            {
                return;
            }

            try
            {
                ApplyMovement();
                ApplyRotation();
            }
            catch (Exception e)
            {
                Debug.LogError($"[MovingState] FixedUpdate failed: {e.Message}");
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Applies movement to the character based on input
        /// </summary>
        private void ApplyMovement()
        {
            var moveDirection = GetMoveDirection();
            var moveSpeed = GetMoveSpeed();
            _playerComponents.CharacterActor.PlanarVelocity = moveDirection * moveSpeed;
        }

        /// <summary>
        /// Applies rotation to face the movement direction
        /// </summary>
        private void ApplyRotation()
        {
            var moveDirection = GetMoveDirection();
            Quaternion targetRotation = default;

            if (_inputManager.AimInput)
                targetRotation = Quaternion.LookRotation(_playerComponents.PlayerCamera.transform.forward);

            if (moveDirection.magnitude > ROTATION_THRESHOLD && !_inputManager.AimInput)
                targetRotation = Quaternion.Euler(0, _playerComponents.PlayerCamera.transform.eulerAngles.y, 0);
            
            if (moveDirection.magnitude <= MOVING_THRESHOLD && !_inputManager.AimInput)
                return;

            //var targetRotation = Quaternion.LookRotation(moveDirection);
            //targetRotation = _inputManager.AimInput ? Quaternion.LookRotation(_playerComponents.PlayerCamera.transform.forward) : Quaternion.Euler(0, _playerComponents.PlayerCamera.transform.eulerAngles.y, 0);
            var currentRotation = _playerComponents.CharacterActor.Rotation;
            var newRotation = Quaternion.Slerp(currentRotation, targetRotation, ROTATION_SPEED * Time.fixedDeltaTime);
            
            _playerComponents.CharacterActor.Rotation = newRotation;
        }

        /// <summary>
        /// Calculates movement direction relative to camera
        /// </summary>
        /// <returns>Normalized movement direction</returns>
        private Vector3 GetMoveDirection()
        {
            if (_playerComponents?.PlayerCamera?.transform == null)
                return Vector3.zero;

            var cameraTransform = _playerComponents.PlayerCamera.transform;
            var movementInput = _inputManager.MovementInput;

            var moveDirection = cameraTransform.forward * movementInput.y + 
                               cameraTransform.right * movementInput.x;
            
            moveDirection.y = 0f; // Remove vertical component
            return moveDirection.normalized;
        }

        /// <summary>
        /// Determines movement speed based on sprint input
        /// </summary>
        /// <returns>Current movement speed</returns>
        private float GetMoveSpeed()
        {
            return _inputManager.SprintInput ? RUN_SPEED : WALK_SPEED;
        }
        #endregion
    }
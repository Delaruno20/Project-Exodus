using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
    /// Base implementation of a hierarchical state that can contain child states and activities.
    /// This class provides the foundation for a robust state machine system with proper error handling,
    /// async activity management, and transition support.
    /// </summary>
public abstract class BaseState : IState
{
        #region Constants
        private const string ERROR_NULL_STATE_MACHINE = "StateMachine cannot be null";
        private const string ERROR_ENTER_FAILED = "Failed to enter state";
        private const string ERROR_EXIT_FAILED = "Failed to exit state";
        private const string ERROR_UPDATE_FAILED = "Failed to update state";
        #endregion

        #region Fields
        private readonly List<IActivity> _activities = new List<IActivity>();
        private readonly List<TransitionRule> _transitionRules = new List<TransitionRule>();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;
        #endregion

        #region Properties
        /// <summary>
        /// Reference to the hierarchical state machine that owns this state
        /// </summary>
        public HierarchicalStateMachine StateMachine { get; }

        /// <summary>
        /// Parent state in the hierarchy, null if this is the root state
        /// </summary>
        public IState ParentState { get; }

        /// <summary>
        /// Currently active child state, null if no child is active
        /// </summary>
        public IState ChildState { get; set; }

        /// <summary>
        /// Read-only collection of activities associated with this state
        /// </summary>
        public IReadOnlyList<IActivity> Activities => _activities.AsReadOnly();

        /// <summary>
        /// Indicates whether this state is currently active
        /// </summary>
        public bool IsActive { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the BaseState class
        /// </summary>
        /// <param name="stateMachine">The hierarchical state machine that owns this state</param>
        /// <param name="parentState">Optional parent state in the hierarchy</param>
        /// <exception cref="ArgumentNullException">Thrown when stateMachine is null</exception>
        protected BaseState(HierarchicalStateMachine stateMachine, IState parentState = null)
        {
            StateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine), ERROR_NULL_STATE_MACHINE);
            ParentState = parentState;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enters this state, activating all associated activities and child states
        /// </summary>
        public async void Enter()
        {
            if (IsActive)
            {
                Debug.LogWarning($"[{GetStateName()}] State is already active");
                return;
            }

            try
            {
                IsActive = true;
                SetAsChildOfParent();
                await ActivateActivitiesAsync();
                EnterInitialChild();
                OnEnter();
                
                Debug.Log($"[{GetStateName()}] Successfully entered state");
            }
            catch (Exception e)
            {
                IsActive = false;
                LogError(ERROR_ENTER_FAILED, e);
                await HandleEnterFailure();
            }
        }

        /// <summary>
        /// Exits this state, deactivating all activities and child states
        /// </summary>
        public async void Exit()
        {
            if (!IsActive)
            {
                Debug.LogWarning($"[{GetStateName()}] State is not active");
                return;
            }

            try
            {
                ExitChildState();
                await DeactivateActivitiesAsync();
                OnExit();
                IsActive = false;
                
                Debug.Log($"[{GetStateName()}] Successfully exited state");
            }
            catch (Exception e)
            {
                LogError(ERROR_EXIT_FAILED, e);
            }
        }

        /// <summary>
        /// Updates this state and handles transitions
        /// </summary>
        public void Update()
        {
            if (!IsActive) return;

            try
            {
                HandleTransitions();
                UpdateChildState();
                OnUpdate();
            }
            catch (Exception e)
            {
                LogError(ERROR_UPDATE_FAILED, e);
            }
        }

        /// <summary>
        /// Fixed update for physics-related operations
        /// </summary>
        public void FixedUpdate()
        {
            if (!IsActive) return;

            try
            {
                ChildState?.FixedUpdate();
                OnFixedUpdate();
            }
            catch (Exception e)
            {
                LogError("Fixed update failed", e);
            }
        }

        /// <summary>
        /// Late update for operations that need to occur after all Updates
        /// </summary>
        public void LateUpdate()
        {
            if (!IsActive) return;

            try
            {
                ChildState?.LateUpdate();
                OnLateUpdate();
            }
            catch (Exception e)
            {
                LogError("Late update failed", e);
            }
        }

        /// <summary>
        /// Adds an activity to this state with validation
        /// </summary>
        /// <param name="activity">Activity to add</param>
        protected void AddActivity(IActivity activity)
        {
            if (activity == null)
            {
                Debug.LogWarning($"[{GetStateName()}] Cannot add null activity");
                return;
            }

            if (_activities.Contains(activity))
            {
                Debug.LogWarning($"[{GetStateName()}] Activity already exists: {activity.GetType().Name}");
                return;
            }

            _activities.Add(activity);
            Debug.Log($"[{GetStateName()}] Added activity: {activity.GetType().Name}");
        }

        /// <summary>
        /// Adds a transition rule to this state with validation
        /// </summary>
        /// <param name="rule">Transition rule to add</param>
        protected void AddTransitionRule(TransitionRule rule)
        {
            if (rule == null)
            {
                Debug.LogWarning($"[{GetStateName()}] Cannot add null transition rule");
                return;
            }

            if (_transitionRules.Contains(rule))
            {
                Debug.LogWarning($"[{GetStateName()}] Transition rule already exists");
                return;
            }

            _transitionRules.Add(rule);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Sets this state as a child of its parent state
        /// </summary>
        private void SetAsChildOfParent()
        {
            if (ParentState != null)
            {
                ParentState.ChildState = this;
            }
        }

        /// <summary>
        /// Enters the initial child state if one exists
        /// </summary>
        private void EnterInitialChild()
        {
            var initialChild = GetInitialChild();
            if (initialChild != null)
            {
                initialChild.Enter();
            }
        }

        /// <summary>
        /// Exits the current child state
        /// </summary>
        private void ExitChildState()
        {
            ChildState?.Exit();
            ChildState = null;
        }

        /// <summary>
        /// Handles state transitions based on transition rules
        /// </summary>
        private void HandleTransitions()
        {
            var transitionTarget = GetTransitionState();
            if (transitionTarget != null)
            {
                StateMachine.TransitionManager?.RequestTransition(this, transitionTarget);
                ChildState = transitionTarget;
            }
        }

        /// <summary>
        /// Updates the current child state
        /// </summary>
        private void UpdateChildState()
        {
            ChildState?.Update();
        }

        /// <summary>
        /// Activates all activities associated with this state
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        private async Task ActivateActivitiesAsync()
        {
            if (_activities.Count == 0) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var activationTasks = new List<Task>();
            
            foreach (var activity in _activities)
            {
                activationTasks.Add(SafeActivateActivityAsync(activity, _cancellationTokenSource.Token));
            }

            await Task.WhenAll(activationTasks);
        }

        /// <summary>
        /// Safely activates a single activity with error handling
        /// </summary>
        /// <param name="activity">Activity to activate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        private async Task SafeActivateActivityAsync(IActivity activity, CancellationToken cancellationToken)
        {
            try
            {
                await activity.ActivateAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[{GetStateName()}] Activity activation cancelled: {activity.GetType().Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[{GetStateName()}] Failed to activate activity {activity.GetType().Name}: {e.Message}");
            }
        }

        /// <summary>
        /// Deactivates all activities associated with this state
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        private async Task DeactivateActivitiesAsync()
        {
            if (_activities.Count == 0) return;

            var deactivationTasks = new List<Task>();
            
            foreach (var activity in _activities)
            {
                deactivationTasks.Add(SafeDeactivateActivityAsync(activity, _cancellationTokenSource?.Token ?? CancellationToken.None));
            }

            await Task.WhenAll(deactivationTasks);
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        /// <summary>
        /// Safely deactivates a single activity with error handling
        /// </summary>
        /// <param name="activity">Activity to deactivate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        private async Task SafeDeactivateActivityAsync(IActivity activity, CancellationToken cancellationToken)
        {
            try
            {
                await activity.DeactivateAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[{GetStateName()}] Activity deactivation cancelled: {activity.GetType().Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[{GetStateName()}] Failed to deactivate activity {activity.GetType().Name}: {e.Message}");
            }
        }

        /// <summary>
        /// Handles cleanup when state entry fails
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        private async Task HandleEnterFailure()
        {
            try
            {
                ExitChildState();
                await DeactivateActivitiesAsync();
            }
            catch (Exception cleanupException)
            {
                Debug.LogError($"[{GetStateName()}] Cleanup failed after enter failure: {cleanupException.Message}");
            }
        }

        /// <summary>
        /// Gets the name of this state for logging purposes
        /// </summary>
        /// <returns>State name</returns>
        private string GetStateName()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Logs an error with consistent formatting
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="exception">Exception that occurred</param>
        private void LogError(string message, Exception exception)
        {
            Debug.LogError($"[{GetStateName()}] {message}: {exception.Message}\n{exception.StackTrace}");
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Gets the initial child state for this state
        /// Override this method to specify which child state should be entered first
        /// </summary>
        /// <returns>Initial child state or null if no child should be entered</returns>
        public virtual IState GetInitialChild() => null;

        /// <summary>
        /// Evaluates transition rules to determine if a state transition should occur
        /// </summary>
        /// <returns>Target state for transition or null if no transition should occur</returns>
        public virtual IState GetTransitionState()
        {
            foreach (var transitionRule in _transitionRules)
            {
                try
                {
                    if (transitionRule.Condition?.Evaluate() == true)
                    {
                        return transitionRule.TargetState;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[{GetStateName()}] Error evaluating transition rule: {e.Message}");
                }
            }
            return null;
        }

        /// <summary>
        /// Called when the state is entered. Override for custom enter behavior.
        /// </summary>
        protected virtual void OnEnter() { }

        /// <summary>
        /// Called when the state is exited. Override for custom exit behavior.
        /// </summary>
        protected virtual void OnExit() { }

        /// <summary>
        /// Called during Update. Override for custom update behavior.
        /// </summary>
        protected virtual void OnUpdate() { }

        /// <summary>
        /// Called during FixedUpdate. Override for custom physics update behavior.
        /// </summary>
        protected virtual void OnFixedUpdate() { }

        /// <summary>
        /// Called during LateUpdate. Override for custom late update behavior.
        /// </summary>
        protected virtual void OnLateUpdate() { }
        #endregion

        #region IDisposable Implementation
        /// <summary>
        /// Disposes of resources used by this state
        /// </summary>
        public virtual void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                if (IsActive)
                {
                    Exit();
                }

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _activities.Clear();
                _transitionRules.Clear();
            }
            catch (Exception e)
            {
                Debug.LogError($"[{GetStateName()}] Error during disposal: {e.Message}");
            }
            finally
            {
                _isDisposed = true;
            }
        }
        #endregion
}

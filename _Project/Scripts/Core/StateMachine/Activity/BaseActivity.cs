using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
    /// Base implementation for all activities with proper lifecycle management
    /// </summary>
    public abstract class BaseActivity : IActivity
    {
        #region Properties
        /// <summary>
        /// Current state of the activity
        /// </summary>
        public ActivityState State { get; protected set; } = ActivityState.Inactive;

        /// <summary>
        /// Name of the activity for logging purposes
        /// </summary>
        protected virtual string ActivityName => GetType().Name;
        #endregion

        #region Public Methods
        /// <summary>
        /// Activates the activity asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel activation</param>
        /// <returns>Task representing the async operation</returns>
        public async Task ActivateAsync(CancellationToken cancellationToken)
        {
            if (State != ActivityState.Inactive)
            {
                Debug.LogWarning($"[{ActivityName}] Activity is not inactive (current state: {State})");
                return;
            }

            State = ActivityState.Activating;

            try
            {
                await OnActivateAsync(cancellationToken);
                
                if (!cancellationToken.IsCancellationRequested)
                {
                    State = ActivityState.Active;
                    Debug.Log($"[{ActivityName}] Activity activated successfully");
                }
            }
            catch (OperationCanceledException)
            {
                State = ActivityState.Inactive;
                Debug.Log($"[{ActivityName}] Activity activation cancelled");
                throw;
            }
            catch (Exception e)
            {
                State = ActivityState.Faulted;
                Debug.LogError($"[{ActivityName}] Activity activation failed: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deactivates the activity asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel deactivation</param>
        /// <returns>Task representing the async operation</returns>
        public async Task DeactivateAsync(CancellationToken cancellationToken)
        {
            if (State != ActivityState.Active && State != ActivityState.Faulted)
            {
                Debug.LogWarning($"[{ActivityName}] Activity is not active (current state: {State})");
                return;
            }

            State = ActivityState.Deactivating;

            try
            {
                await OnDeactivateAsync(cancellationToken);
                
                if (!cancellationToken.IsCancellationRequested)
                {
                    State = ActivityState.Inactive;
                    Debug.Log($"[{ActivityName}] Activity deactivated successfully");
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[{ActivityName}] Activity deactivation cancelled");
                throw;
            }
            catch (Exception e)
            {
                State = ActivityState.Faulted;
                Debug.LogError($"[{ActivityName}] Activity deactivation failed: {e.Message}");
                throw;
            }
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Override this method to implement custom activation logic
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        protected virtual async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Override this method to implement custom deactivation logic
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        protected virtual async Task OnDeactivateAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
        #endregion
    }
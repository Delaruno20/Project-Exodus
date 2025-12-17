using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
    /// Activity that runs continuously in a loop with configurable update frequency
    /// </summary>
    public abstract class ContinuousActivity : BaseActivity
    {
        #region Constants
        private const int DEFAULT_UPDATE_FREQUENCY_MS = 16; // ~60 FPS
        private const int DEACTIVATION_DELAY_MS = 50;
        #endregion

        #region Fields
        private CancellationTokenSource _loopTokenSource;
        private readonly int _updateFrequencyMs;
        private Task _updateLoopTask;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new ContinuousActivity
        /// </summary>
        /// <param name="updateFrequencyMs">Update frequency in milliseconds (default: 16ms for ~60 FPS)</param>
        public ContinuousActivity(int updateFrequencyMs = DEFAULT_UPDATE_FREQUENCY_MS)
        {
            _updateFrequencyMs = Math.Max(1, updateFrequencyMs);
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Starts the continuous update loop
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _loopTokenSource = new CancellationTokenSource();
            var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _loopTokenSource.Token);

            try
            {
                OnLoopStart();
                _updateLoopTask = UpdateLoop(combinedTokenSource.Token);
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{ActivityName}] Failed to start continuous activity: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stops the continuous update loop
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        protected override async Task OnDeactivateAsync(CancellationToken cancellationToken)
        {
            try
            {
                _loopTokenSource?.Cancel();
                
                if (_updateLoopTask != null)
                {
                    await _updateLoopTask;
                }

                OnLoopEnd();
                await Task.Delay(DEACTIVATION_DELAY_MS, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception e)
            {
                Debug.LogError($"[{ActivityName}] Error during continuous activity deactivation: {e.Message}");
                throw;
            }
            finally
            {
                _loopTokenSource?.Dispose();
                _loopTokenSource = null;
                _updateLoopTask = null;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Main update loop that runs continuously while the activity is active
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        private async Task UpdateLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        OnLoopUpdate();
                        await Task.Delay(_updateFrequencyMs, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[{ActivityName}] Error in update loop: {e.Message}");
                        // Continue the loop despite the error
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when the activity is being deactivated
            }
            catch (Exception e)
            {
                Debug.LogError($"[{ActivityName}] Critical error in update loop: {e.Message}");
            }
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Called when the loop starts. Override for initialization logic.
        /// </summary>
        protected virtual void OnLoopStart() { }

        /// <summary>
        /// Called continuously during the update loop. Override with main activity logic.
        /// </summary>
        protected abstract void OnLoopUpdate();

        /// <summary>
        /// Called when the loop ends. Override for cleanup logic.
        /// </summary>
        protected virtual void OnLoopEnd() { }
        #endregion
    }

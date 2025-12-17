using System;
using UnityEngine;
using Lightbug.CharacterControllerPro.Core;

/// <summary>
/// Main controller for the player that manages the hierarchical state machine
/// and coordinates all player components
/// </summary>
[RequireComponent(typeof(CharacterActor))] 
public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Player Components")]
    [SerializeField, Tooltip("Camera used for player movement calculations")]
    private Camera playerCamera;
        
    [SerializeField, Tooltip("Character controller for physics-based movement")]
    private CharacterActor characterActor;

    [Header("Debug")]
    [SerializeField, Tooltip("Enable debug logging for state machine")]
    private bool enableDebugLogging = false;
    #endregion

    #region Fields
    private HierarchicalStateMachine _hierarchicalStateMachine;
    private PlayerRootState _playerRootState;
    private PlayerComponents _playerComponents;
    private bool _isInitialized;
    #endregion

    #region Properties
    /// <summary>
    /// Player camera reference
    /// </summary>
    public Camera PlayerCamera => playerCamera;

    /// <summary>
    /// Character actor reference
    /// </summary>
    public CharacterActor CharacterActor => characterActor;

    /// <summary>
    /// Hierarchical state machine instance
    /// </summary>
    public HierarchicalStateMachine HierarchicalStateMachine => _hierarchicalStateMachine;

    /// <summary>
    /// Indicates if the player controller is properly initialized
    /// </summary>
    public bool IsInitialized => _isInitialized;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initialize components and state machine on Awake
    /// </summary>
    private void Awake()
    {
        try
        {
                ValidateComponents();
                InitializeComponents();
                InitializePlayerStateMachine();
                _isInitialized = true;

                if (enableDebugLogging)
                {
                    Debug.Log("[PlayerController] Successfully initialized");
                }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerController] Initialization failed: {e.Message}");
            _isInitialized = false;
        }
    }

    /// <summary>
    /// Start the state machine
    /// </summary>
    private void Start()
    {
        if (!_isInitialized)
        {
            Debug.LogError("[PlayerController] Cannot start - initialization failed");
            return;
        }

        try
        {
            _hierarchicalStateMachine?.Start();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerController] Failed to start state machine: {e.Message}");
        }
    }

    /// <summary>
    /// Update the state machine
    /// </summary>
    private void Update()
    {
        if (!_isInitialized) return;

        try
        {
            _hierarchicalStateMachine?.Update();
            Debug.Log(GetCurrentStatePath());
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerController] Update failed: {e.Message}");
        }
    }

    /// <summary>
    /// Fixed update for physics-related state machine updates
    /// </summary>
    private void FixedUpdate()
    {
        if (!_isInitialized) return;

        try
        {
            _hierarchicalStateMachine?.FixedUpdate();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerController] FixedUpdate failed: {e.Message}");
        }
    }

    /// <summary>
    /// Late update for state machine
    /// </summary>
    private void LateUpdate()
    {
        if (!_isInitialized) return;

        try
        {
            _hierarchicalStateMachine?.LateUpdate();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerController] LateUpdate failed: {e.Message}");
        }
    }

    /// <summary>
    /// Clean up resources when destroyed
    /// </summary>
    private void OnDestroy()
    {
        try
        {
            _playerRootState?.Dispose();
            _hierarchicalStateMachine?.Dispose();
                
            if (enableDebugLogging)
            {
                Debug.Log("[PlayerController] Successfully cleaned up");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerController] Cleanup failed: {e.Message}");
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Validates that all required components are assigned
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required components are missing</exception>
    private void ValidateComponents()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                throw new InvalidOperationException("Player camera is not assigned and no main camera found");
            }
            Debug.LogWarning("[PlayerController] Player camera not assigned, using main camera");
        }

        if (characterActor == null)
        {
            characterActor = GetComponent<CharacterActor>();
            if (characterActor == null)
            {
                throw new InvalidOperationException("CharacterActor component is required");
            }
        }
    }

    /// <summary>
    /// Initializes player components and registers them with dependency injection
    /// </summary>
    private void InitializeComponents()
    {
        try
        {
            _hierarchicalStateMachine = new HierarchicalStateMachine(null);
            _playerComponents = new PlayerComponents(characterActor, _hierarchicalStateMachine, playerCamera);

            // Register components with dependency injection
            var container = DependencyInjectionManager.Instance?.Container;
            if (container != null)
            {
                container.RegisterSingleton(_playerComponents);
            }
            else
            {
                Debug.LogWarning("[PlayerController] Dependency injection manager not available");
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to initialize components: {e.Message}", e);
        }
    }

    /// <summary>
    /// Initializes the player state machine hierarchy
    /// </summary>
    private void InitializePlayerStateMachine()
    {
        try
        {
            if (_playerComponents == null)
            {
                throw new InvalidOperationException("Player components not initialized");
            }

            _playerRootState = new PlayerRootState(_hierarchicalStateMachine, _playerComponents);
            _hierarchicalStateMachine.SetRootState(_playerRootState);

            if (enableDebugLogging)
            {
                Debug.Log("[PlayerController] State machine initialized with root state");
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to initialize state machine: {e.Message}", e);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Gets the current active state path for debugging
    /// </summary>
    /// <returns>String representation of the current state hierarchy</returns>
    private string GetCurrentStatePath()
    {
        if (!_isInitialized || _hierarchicalStateMachine == null)
        {
            return "Not Initialized";
        }

        return _hierarchicalStateMachine.GetCurrentStatePath();
    }

    /// <summary>
    /// Forces a transition to a specific state (for debugging purposes)
    /// </summary>
    /// <param name="targetState">Target state to transition to</param>
    /// <returns>True if transition was requested successfully</returns>
    public bool ForceTransition(IState targetState)
    {
        if (!_isInitialized || _hierarchicalStateMachine?.TransitionManager == null)
        {
            Debug.LogError("[PlayerController] Cannot force transition - not properly initialized");
            return false;
        }

        try
        {
            _hierarchicalStateMachine.TransitionManager.RequestTransition(_playerRootState, targetState);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerController] Force transition failed: {e.Message}");
            return false;
        }
    }
    #endregion

    #region Editor Support
#if UNITY_EDITOR
    /// <summary>
    /// Draws gizmos for debugging in the scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_isInitialized) return;

        // Draw movement direction
        if (_playerComponents?.CharacterActor != null)
        {
            var velocity = _playerComponents.CharacterActor.PlanarVelocity;
            if (velocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position + Vector3.up, velocity.normalized * 2f);
            }
        }
    }

    /// <summary>
    /// Validates the setup in the editor
    /// </summary>
    private void OnValidate()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main;
        }

        if (characterActor == null)
        {
            characterActor = GetComponent<CharacterActor>();
        }
    }
#endif
    #endregion
}
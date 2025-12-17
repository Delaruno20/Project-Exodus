using System;
using UnityEngine;

/// <summary>
/// Root state for the player state machine hierarchy
/// </summary>
public class PlayerRootState : BaseState
{
    #region Fields
    private readonly AliveState _aliveState;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new PlayerRootState
    /// </summary>
    /// <param name="stateMachine">The state machine</param>
    /// <param name="playerComponents">Player components reference</param>
    /// <param name="parentState">Optional parent state (should be null for root)</param>
    public PlayerRootState(HierarchicalStateMachine stateMachine, PlayerComponents playerComponents, IState parentState = null) 
        : base(stateMachine, parentState)
    {
        if (playerComponents == null)
        {
            throw new ArgumentNullException(nameof(playerComponents));
        }

        try
        {
            _aliveState = new AliveState(stateMachine, playerComponents, this);
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerRootState] Constructor failed: {e.Message}");
        }
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Returns the initial child state (AliveState)
    /// </summary>
    /// <returns>Alive state as initial child</returns>
    public override IState GetInitialChild()
    {
        return _aliveState;
    }
    #endregion
}
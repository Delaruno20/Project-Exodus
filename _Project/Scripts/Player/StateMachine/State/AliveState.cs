using System;
using UnityEngine;

/// <summary>
/// Represents the alive state of the player, containing movement and other living behaviors
/// </summary>
public class AliveState : BaseState
{
    #region Fields
    private readonly MovingState _movingState;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new AliveState
    /// </summary>
    /// <param name="stateMachine">The state machine</param>
    /// <param name="playerComponents">Player components reference</param>
    /// <param name="parentState">Optional parent state</param>
    public AliveState(HierarchicalStateMachine stateMachine, PlayerComponents playerComponents, IState parentState = null) 
        : base(stateMachine, parentState)
    {
        if (playerComponents == null)
        {
            throw new ArgumentNullException(nameof(playerComponents));
        }

        try
        {
            _movingState = new MovingState(stateMachine, playerComponents, this);
        }
        catch (Exception e)
        {
            Debug.LogError($"[AliveState] Constructor failed: {e.Message}");
        }
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Returns the initial child state (MovingState)
    /// </summary>
    /// <returns>Moving state as initial child</returns>
    public override IState GetInitialChild()
    {
        return _movingState;
    }
    #endregion
}
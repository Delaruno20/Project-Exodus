using UnityEngine;

public interface IInputManager
{
    Vector2 MovementInput { get; }
    Vector2 LookInput { get; }
    bool SprintInput { get; }
    bool JumpInput { get; }
    bool AimInput { get; }
}
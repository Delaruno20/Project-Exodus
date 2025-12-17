using System;
using UnityEngine;

public class InputManager : IInputManager
{
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool AimInput { get; private set; }
    private readonly InputControls _controls;

    public InputManager()
    {
        _controls = new InputControls();
        _controls.Player.Move.performed += ctx => MovementInput = ctx.ReadValue<Vector2>();
        _controls.Player.Move.canceled += ctx => MovementInput = Vector2.zero;
        _controls.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        _controls.Player.Look.canceled += ctx => LookInput = Vector2.zero;
        _controls.Player.Aim.performed += ctx => AimInput = ctx.ReadValueAsButton();
        _controls.Player.Aim.canceled += ctx => AimInput = false;
        _controls.Player.Sprint.performed += ctx => SprintInput = ctx.ReadValueAsButton();
        _controls.Player.Sprint.canceled += ctx => SprintInput = false;
        _controls.Player.Jump.performed += ctx => JumpInput = ctx.ReadValueAsButton();
        _controls.Player.Jump.canceled += ctx => JumpInput = false;

        _controls.Player.Enable();
    }


    public void Update()
    {
        /*
        MovementInput = _controls.Player.Move.ReadValue<Vector2>();
        LookInput = _controls.Player.Look.ReadValue<Vector2>();
        SprintInput = _controls.Player.Sprint.IsPressed();
        JumpInput = _controls.Player.Jump.IsPressed();
        AimInput = _controls.Player.Aim.IsPressed();
        */
    }
}
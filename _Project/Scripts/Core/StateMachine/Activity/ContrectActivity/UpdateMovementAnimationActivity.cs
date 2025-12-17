using UnityEngine;

public class UpdateMovementAnimationActivity : ContinuousActivity
{
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    [InjectProperty] private IAnimatorService _animatorService;
    [InjectProperty] private readonly IInputManager _inputManager;
    protected override void OnLoopUpdate()
    {
        UpdateAnimatorValues(_inputManager.MovementInput.y,_inputManager.MovementInput.x, _inputManager.SprintInput);
    }

    public void Update()
    {
        if (_animatorService == null)
        {
            Debug.LogError("AnimatorService is null");
            return;
        }

        if (_inputManager == null)
        {
            Debug.LogError("InputManager is null");
            return;
        }
        OnLoopUpdate();
    }
    
    public void UpdateAnimatorValues(float horizontal, float vertical, bool isSprinting)
    {
        if (isSprinting)
        {
            horizontal *= 2f;
            vertical *= 2f;
        }
        _animatorService.Animator.SetFloat(Horizontal, horizontal, 0.1f, Time.deltaTime);
        _animatorService.Animator.SetFloat(Vertical, vertical, 0.1f, Time.deltaTime);
    }
}
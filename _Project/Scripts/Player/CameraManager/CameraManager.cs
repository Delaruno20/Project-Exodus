using System;
using UnityEngine;

public class CameraManager : MonoBehaviour, ICameraManager
{
    [SerializeField] private GameObject normalCamera;
    [SerializeField] private GameObject aimCamera;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float sensitivity = 1.5f;
    [SerializeField] private float minimumY = -60f;
    [SerializeField] private float maximumY = 60f;
    [SerializeField] private float aimSensitivity = 0.5f;
    [SerializeField] private float aimMinimumY = -30f;
    [SerializeField] private float aimMaximumY = 30f;
    [SerializeField] private float smoothSpeed = 0.125f;
    [InjectProperty] private IInputManager _inputManager;
    
    
    private float _xRotation;
    private float _yRotation;
    private bool _isAiming;

    private void Start()
    {
        DependencyInjectionManager.Instance.InjectMonoBehaviour(this);
    }

    private void OnValidate()
    {
        if (cameraPivot == null)
        {
            cameraPivot = transform;
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
        SwitchCamera();
    }


    public float GetCurrentSensitivity()
    {
        return _inputManager.AimInput ? aimSensitivity : sensitivity;
    }

    public float GetCurrentMinimumY()
    {
        return _inputManager.AimInput ? aimMinimumY : minimumY;
    }

    public float GetCurrentMaximumY()
    {
        return _inputManager.AimInput ? aimMaximumY : maximumY;
    }

    public void CameraRotation()
    {
        float currentSensitivity = GetCurrentSensitivity();
        float minY = GetCurrentMinimumY();
        float maxY = GetCurrentMaximumY();

        _xRotation -= _inputManager.LookInput.y * currentSensitivity;
        _yRotation += _inputManager.LookInput.x * currentSensitivity;

        _xRotation = Mathf.Clamp(_xRotation, minY, maxY);
        _yRotation = Mathf.Repeat(_yRotation, 360f);

        Quaternion targetRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
        cameraPivot.rotation = Quaternion.Slerp(cameraPivot.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    public void SwitchCamera()
    {
        if (_inputManager.AimInput == _isAiming) return;

        _isAiming = _inputManager.AimInput;
        normalCamera.SetActive(!_isAiming);
        aimCamera.SetActive(_isAiming);
    }
}
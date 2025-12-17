public interface ICameraManager
{
    float GetCurrentSensitivity();
    float GetCurrentMinimumY();
    float GetCurrentMaximumY();
    void CameraRotation();
    void SwitchCamera();
}
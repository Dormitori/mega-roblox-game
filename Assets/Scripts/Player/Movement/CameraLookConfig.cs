using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player/CameraLookConfig")]
public class CameraLookConfig : ScriptableObject
{
    public float mouseSensitivity;
    public float touchSensitivity;
    
    public float cameraDistance;
    public float cameraScrollWheelStep;
    public float cameraScrollMobileStep;
    public float cameraMaxDistance;
    public float cameraMinDistance;
    public float cameraAnimationSpeed;

    public float startXRotation;
    public float startYRotation;


    public float xMaxClamp;
    public float xMinClamp;
}
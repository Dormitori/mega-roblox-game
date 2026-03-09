using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player/CameraLookConfig")]
public class CameraLookConfig : ScriptableObject
{
    public float sensitivity;
    
    public float cameraDistance;
    public float cameraStep;
    public float cameraMaxDistance;
    public float cameraMinDistance;
    public float cameraAnimationSpeed;

    public float startXRotation;
    public float startYRotation;


    public float xMaxClamp;
    public float xMinClamp;
}
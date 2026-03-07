using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player/CameraLookConfig")]
public class CameraLookConfig : ScriptableObject
{
    public float sensitivity;
    public float cameraDistance;
    
    public float startXRotation;
    public float startYRotation;

    public float xMaxClamp;
    public float xMinClamp;
}
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Gameplay/BlockAnimationConfig")]
public class BlockAnimationConfig : ScriptableObject
{
    [Header("Hit Animation Settings")]
    public float hitScaleDuration = 0.2f;
    public float hitScaleAmount = 0.15f;
    public Ease hitScaleEase = Ease.OutBack;
    
    [Header("Highlight Animation Settings")]
    public float highlightScaleAmount = 0f;
    public float highlightDuration = 0.3f;
    public Ease highlightEase = Ease.OutQuad;
    
    [Header("Disable Animation Settings")]
    public float disableScaleAmount = 0.9f;
    public float disableDuration = 0.3f;
    public Ease disableEase = Ease.InQuad;
    
    [Header("Enable Animation Settings")]
    public float enableDuration = 0.3f;
    public Ease enableEase = Ease.OutBack;
    
    [Header("Destroy Animation Settings")]
    public float destroyDuration = 0.5f;
    public Ease destroyEase = Ease.InBack;
}

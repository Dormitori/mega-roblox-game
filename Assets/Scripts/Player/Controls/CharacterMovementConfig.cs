using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player/CharacterMovementConfig")]
public class CharacterMovementConfig : ScriptableObject
{
    public float speed;

    public float jumpHeight;

    public float jumpTime;
    public float fallingTime;

    public float jumpBufferTime;
    public float coyoteTime;

    public float rotationSpeed;

    public float beforeHitCooldown = 0.4f;
    public float afterHitCooldown = 0.4f;

    public float attackAnimationClipLengthSeconds = 0.967f;

    public float attackAnimatorStateSpeedMultiplier = 1.5f;

    public float miningAttackMovementTail = 0f;

    public float UpwardGravity => 2 * jumpHeight / (jumpTime * jumpTime);
    public float DownwardGravity => 2 * jumpHeight / (fallingTime * fallingTime);
    public float JumpVelocity => 2 * jumpHeight / jumpTime;
}
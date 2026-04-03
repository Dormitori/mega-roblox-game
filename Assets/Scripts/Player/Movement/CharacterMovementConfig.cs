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

    [Tooltip("Длительность клипа атаки при скорости 1.0 (см. AnimationClip). Используется чтобы выставить AttackSpeed так, чтобы анимация укладывалась в before+after.")]
    public float attackAnimationClipLengthSeconds = 0.967f;

    [Tooltip("Множитель Speed у стейта Attack в Animator (не параметр). Эффективная скорость клипа = это значение × float AttackSpeed.")]
    public float attackAnimatorStateSpeedMultiplier = 1.5f;

    [Tooltip("После afterHitCooldown дополнительная блокировка движения (сек). Оставь 0, если длительность уже подогнана через attackAnimationClipLengthSeconds.")]
    public float miningAttackMovementTail = 0f;

    public float UpwardGravity => 2 * jumpHeight / (jumpTime * jumpTime);
    public float DownwardGravity => 2 * jumpHeight / (fallingTime * fallingTime);
    public float JumpVelocity => 2 * jumpHeight / jumpTime;
}
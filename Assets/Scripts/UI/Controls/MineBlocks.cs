using System.Collections;
using Core.Audio;
using UnityEngine;
using VContainer;

public class MineBlocks : MonoBehaviour
{
    public CharacterMovementConfig config;
    public LayerMask blockLayer;
    public PlayerPickaxe playerPickaxe;
    public PlayerBlockInventory playerBlockInventory;
    public Transform minePoint;
    public InputControls inputControls;
    public Animator animator;
    public ParticleSystem attackParticle;
    public float mineDistance;

    [SerializeField, Range(0f, 0.2f)] private float mineHitPitchJitterHalfRange = 0.06f;

    public bool IsMiningAttacking => _isHitting;

    private IAudioService _audioService;

    private RaycastHit _currentBlockHit;
    private Block _currentBlock;
    private bool _isHitting;

    [Inject]
    private void Construct(IAudioService audioService)
    {
        _audioService = audioService;
    }

    private void Update()
    {
        if (GetHit(out RaycastHit hit))
        {
            if (hit.collider != _currentBlockHit.collider)
            {
                ClearHighLight();
                var block = hit.collider.GetComponent<Block>();
                if (block && !block.IsDisabled)
                {
                    block.Highlight();
                    _currentBlockHit = hit;
                    _currentBlock = block;
                }
                else
                {
                    _currentBlockHit = default;
                    _currentBlock = null;
                }
            }
        }
        else
        {
            ClearHighLight();
            _currentBlockHit = default;
            _currentBlock = null;
        }
        
        if (inputControls.MineIsPressed() && !_isHitting)
        {
            StartCoroutine(HitCoroutine());
        }
    }

    private IEnumerator HitCoroutine()
    {
        _isHitting = true;
        _audioService?.PlaySfx(SoundId.BlockHit, 1f, mineHitPitchJitterHalfRange);

        var hitSpeed = playerPickaxe.CurrentPickaxeConfig.baseSpeedMultiplier;
        var attackDuration = (config.beforeHitCooldown + config.afterHitCooldown) / hitSpeed;
        var clipLen = config.attackAnimationClipLengthSeconds;
        var stateMul = config.attackAnimatorStateSpeedMultiplier;
        if (clipLen > 1e-4f && attackDuration > 1e-4f && stateMul > 1e-4f)
            animator.SetFloat("AttackSpeed", clipLen / (stateMul * attackDuration));
        else
            animator.SetFloat("AttackSpeed", hitSpeed);
        animator.SetTrigger("Attack");
        attackParticle.Play();
        yield return new WaitForSeconds(config.beforeHitCooldown / hitSpeed);
        if (_currentBlock && !_currentBlock.IsDisabled && playerBlockInventory.HasSpace)
            _currentBlock.TakeDamage(playerPickaxe.CurrentPickaxeConfig.baseDamage);
        else if (!playerBlockInventory.HasSpace)
            playerBlockInventory.ShowNotEnoughSpaceText();
        
        yield return new WaitForSeconds(config.afterHitCooldown / hitSpeed);
        if (config.miningAttackMovementTail > 0f)
            yield return new WaitForSeconds(config.miningAttackMovementTail);
        _isHitting = false;
    }

    private bool GetHit(out RaycastHit hit)
    {
        var forwardHit = Physics.Raycast(minePoint.position, minePoint.forward, out hit, mineDistance, blockLayer);
        if (forwardHit)
            return true;
        var belowHit = Physics.Raycast(minePoint.position, minePoint.up * -1f, out hit, mineDistance, blockLayer);
        return belowHit;
    }

    private void ClearHighLight()
    {
        if (_currentBlock&& !_currentBlock.IsDisabled)
        {
            _currentBlock.Unhighlight();
        }
    }
}
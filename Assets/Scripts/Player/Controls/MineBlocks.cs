using System.Collections;
using Core.Audio;
using UnityEngine;
using VContainer;

public class MineBlocks : MonoBehaviour
{
    public Block CurrentBlock { get; private set; }

    public CharacterMovementConfig config;
    public LayerMask blockLayer;
    public PlayerPickaxe playerPickaxe;
    public PlayerBlockInventory playerBlockInventory;
    public Transform minePoint;
    public ControlsProvider controlsProvider;
    public Animator animator;
    public ParticleSystem attackParticle;
    public float mineDistance;

    [SerializeField, Range(0f, 0.2f)] private float mineHitPitchJitterHalfRange = 0.06f;
    
    public bool IsMiningAttacking => _isHitting;

    private IAudioService _audioService;
    private PetStatService _petStatService;

    private RaycastHit _currentBlockHit;
    private bool _isHitting;

    [Inject]
    private void Construct(IAudioService audioService, PetStatService petStatService)
    {
        _audioService = audioService;
        _petStatService = petStatService;
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
                    CurrentBlock = block;
                }
                else
                {
                    _currentBlockHit = default;
                    CurrentBlock = null;
                }
            }
        }
        else
        {
            ClearHighLight();
            _currentBlockHit = default;
            CurrentBlock = null;
        }
        
        if (controlsProvider.Controls.MineIsPressed() && !_isHitting)
        {
            StartCoroutine(HitCoroutine());
        }
    }

    private IEnumerator HitCoroutine()
    {
        _isHitting = true;

        var hitSpeed = playerPickaxe.CurrentPickaxeConfig.baseSpeedMultiplier
            * _petStatService.GetMultiplier(PetStatType.AttackSpeedPercent);
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
        
        if (CurrentBlock != null)
        {
            var canMine = playerBlockInventory.HasSpace ||
                          MineChestRules.IgnoresBackpackCapacity(CurrentBlock.InventoryBlockType);
            if (canMine)
            {
                var damage = Mathf.RoundToInt(playerPickaxe.CurrentPickaxeConfig.baseDamage
                    * _petStatService.GetMultiplier(PetStatType.BlockDamagePercent));
                CurrentBlock.TakeDamage(damage);
                _audioService?.PlaySfx(SoundId.BlockHit, 1f, mineHitPitchJitterHalfRange);
            }
            else
                playerBlockInventory.ShowNotEnoughSpaceText();
        }
        
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
        if (CurrentBlock&& !CurrentBlock.IsDisabled)
        {
            CurrentBlock.Unhighlight();
        }
    }
}
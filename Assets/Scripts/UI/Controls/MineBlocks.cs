using System;
using System.Collections;
using UnityEngine;

public class MineBlocks : MonoBehaviour
{
    public CharacterMovementConfig config;
    public LayerMask blockLayer;
    public PlayerPickaxe PlayerPickaxe;
    public Transform minePoint;
    public InputControls inputControls;
    public Animator animator;
    public ParticleSystem attackParticle;
    public float mineDistance;
    
    private RaycastHit _currentBlockHit;
    private Block _currentBlock;
    private bool _isHitting;
    
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
        var hitSpeed = PlayerPickaxe.CurrentPickaxeConfig.baseSpeedMultiplier;
        animator.SetFloat("AttackSpeed", hitSpeed);
        animator.SetTrigger("Attack");
        attackParticle.Play();
        yield return new WaitForSeconds(config.beforeHitCooldown / hitSpeed);
        if (_currentBlock && !_currentBlock.IsDisabled)
            _currentBlock.TakeDamage(PlayerPickaxe.CurrentPickaxeConfig.baseDamage);
        
        yield return new WaitForSeconds(config.afterHitCooldown / hitSpeed);
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
using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Block : MonoBehaviour
{
    public bool IsDisabled;
    public event Action<Block> BlockDestroyed;

    [Header("Block Settings")]
    public Items blockType;
    public int variantId;
    public BlockConfig config;
    public BlockAnimationConfig animationConfig;
    public Transform visualsTransform;
    public Material defaultMaterial;
    public Material highlightMaterial;
    public Material disabledMaterial;
    public Material destroyParticlesMaterial;
    public MeshRenderer meshRenderer;

    private Health _health;
    private Tweener _shakeTween;
    private Tweener _scaleTween;
    private Sequence _hitSequence;
    private Vector3 _originalScale;
    
    private void Awake()
    {
        _health = GetComponent<Health>();
        
        if (IsNotValidComponents()) return;

        _originalScale = visualsTransform.localScale;
        meshRenderer.sharedMaterial = defaultMaterial;
        _health.Death += OnBlockDestroy;
        _health.SetHealth(config.health);
    }

    public void Highlight() 
    {
        if (meshRenderer != null && highlightMaterial != null)
        {
            meshRenderer.sharedMaterial = highlightMaterial;
            
            if (animationConfig != null && animationConfig.highlightScaleAmount > 0)
            {
                _scaleTween?.Kill();
                _scaleTween = visualsTransform.DOScale(
                        _originalScale * (1f + animationConfig.highlightScaleAmount), 
                        animationConfig.highlightDuration)
                    .SetEase(animationConfig.highlightEase)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }
    }

    public void Unhighlight()
    {
        if (meshRenderer != null && defaultMaterial != null)
        {
            meshRenderer.sharedMaterial = defaultMaterial;
        }
        
        _scaleTween?.Kill();
        _scaleTween = visualsTransform.DOScale(_originalScale, 0.2f)
            .SetEase(Ease.OutQuad);
    }

    public void Disable()
    {
        if (meshRenderer != null && disabledMaterial != null)
        {
            meshRenderer.sharedMaterial = disabledMaterial;
        }
        IsDisabled = true;
    }

    public void Enable()
    {
        if (meshRenderer != null && defaultMaterial != null)
        {
            meshRenderer.sharedMaterial = defaultMaterial;
        }
        IsDisabled = false;

        if (visualsTransform == null) return;
        _scaleTween?.Kill();
        visualsTransform.localScale = _originalScale;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        
        _health.TakeDamage(damage);
        
        if (_health.health >= Mathf.Epsilon) 
        {
            PlayHitAnimation();
        }
    }

    private void PlayHitAnimation()
    {
        if (animationConfig == null) return;
        
        _shakeTween?.Kill();
        _scaleTween?.Kill();
        _hitSequence?.Kill();
        
        _hitSequence = DOTween.Sequence();
        
        _hitSequence.Append(visualsTransform.DOScale(
                _originalScale * (1f - animationConfig.hitScaleAmount), 
                animationConfig.hitScaleDuration * 0.3f)
            .SetEase(Ease.InQuad));
        
        _hitSequence.Append(visualsTransform.DOScale(
                _originalScale * (1f + animationConfig.hitScaleAmount * 0.5f), 
                animationConfig.hitScaleDuration * 0.4f)
            .SetEase(animationConfig.hitScaleEase));
        
        _hitSequence.Append(visualsTransform.DOScale(
                _originalScale, 
                animationConfig.hitScaleDuration * 0.3f)
            .SetEase(Ease.OutQuad));
    }

    public void ResetHealth()
    {
        if (_health != null && config != null)
        {
            _health.SetHealth(config.health);
        }
    }
    
    private void OnBlockDestroy()
    {
        _shakeTween?.Kill();
        _scaleTween?.Kill();
        _hitSequence?.Kill();
        
        BlockDestroyed?.Invoke(this);
    }
    
    private void OnDestroy()
    {
        _shakeTween?.Kill();
        _scaleTween?.Kill();
        _hitSequence?.Kill();
    }
    
    private bool IsNotValidComponents()
    {
        if (_health == null)
        {
            Debug.LogError("Health component not found!", this);
            return true;
        }
        
        if (config == null)
        {
            Debug.LogError("Block config is not assigned!", this);
            return true;
        }
        
        if (animationConfig == null)
        {
            Debug.LogWarning("Block animation config is not assigned! Using default values.", this);
        }
        
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            }
        }
        
        if (visualsTransform == null)
        {
            visualsTransform = transform;
        }

        return false;
    }
}
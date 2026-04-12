using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Block : MonoBehaviour
{
    public bool IsDisabled;
    public event Action<Block> BlockDestroyed;
    public event Action<Block, int, int> Damaged;

    [Header("Block Settings")]
    public BlockType blockType;
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

    private int _runtimeMaxHealth;
    private int _runtimeSellPrice;

    public int RuntimeSellPrice => _runtimeSellPrice;

    /// <summary>Тип для инвентаря и экономики (может отличаться от blockType префаба при подмене визуала).</summary>
    public BlockType InventoryBlockType { get; private set; }

    /// <summary>
    /// Вызывается при спавне из шахты: HP/цена из MineBalance, конфиг — для иконки и локализации.
    /// Поля blockType/variantId префаба не трогаем — нужны для возврата в ObjectPool.
    /// </summary>
    public void ApplyMineSpawn(BlockConfig inventoryConfig, int maxHealth, int unitSellPrice)
    {
        config = inventoryConfig;
        InventoryBlockType = inventoryConfig.type;
        _runtimeMaxHealth = Mathf.Max(1, maxHealth);
        _runtimeSellPrice = Mathf.Max(0, unitSellPrice);
        if (_health != null)
            _health.SetHealth(_runtimeMaxHealth);
    }
    
    private void Awake()
    {
        _health = GetComponent<Health>();
        
        if (IsNotValidComponents()) return;

        _originalScale = visualsTransform.localScale;
        meshRenderer.sharedMaterial = defaultMaterial;
        _health.Death += OnBlockDestroy;
        _runtimeMaxHealth = Mathf.Max(1, Mathf.RoundToInt(config.health));
        InventoryBlockType = config.type;
        _health.SetHealth(_runtimeMaxHealth);
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
            var rem = Mathf.RoundToInt(_health.health);
            var maxHp = _runtimeMaxHealth;
            Damaged?.Invoke(this, rem, maxHp);
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
            _health.SetHealth(_runtimeMaxHealth);
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
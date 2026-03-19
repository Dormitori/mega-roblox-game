using System;
using DG.Tweening;
using UnityEngine;


[RequireComponent(typeof(Health))]
public class Block : MonoBehaviour
{
    public bool IsDisabled;
    public event Action<Block> BlockDestroyed;

    public string name;
    public BlockConfig config;
    public Transform visualsTransform;
    public Material defaultMaterial;
    public Material highlightMaterial;
    public Material disabledMaterial;
    public MeshRenderer meshRenderer;

    private Health _health;
    private Tweener _shakeTween;
    
    private void Awake()
    {
        _health = GetComponent<Health>();
        _health.Death += OnBlockDestroy;
        _health.SetHealth(config.health);
    }

    public void Highlight() 
    {
        meshRenderer.material = highlightMaterial;
    }

    public void Unhighlight()
    {
        meshRenderer.material = defaultMaterial;
    }

    public void Disable()
    {
        meshRenderer.material = disabledMaterial;
        IsDisabled = true;
    }

    public void Enable()
    {
        meshRenderer.material = defaultMaterial;
        IsDisabled = false;
    }

    public void TakeDamage(int damage)
    {
        _health.TakeDamage(damage);
        if (_health.health >= Mathf.Epsilon) 
            _shakeTween = visualsTransform.DOShakePosition(0.15f, 0.2f, vibrato:30);
    }

    public void ResetHealth()
    {
        _health.SetHealth(config.health);
    }
    
    private void OnBlockDestroy()
    {
        _shakeTween?.Kill();
        BlockDestroyed?.Invoke(this);
    }
}
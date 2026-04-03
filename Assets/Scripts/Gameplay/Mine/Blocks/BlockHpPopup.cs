using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BlockHpPopup : MonoBehaviour
{
    [SerializeField] private float fontSizeBase = 6f;
    [SerializeField] [Range(0.5f, 1f)] private float currentHpSizePercent = 0.82f;
    [SerializeField] private float floatUpHeight = 0.75f;
    [SerializeField] private float floatDuration = 0.95f;
    [SerializeField] private float fadeStartNormalized = 0.38f;
    [SerializeField] private float spawnOffsetY = 0.42f;
    [SerializeField] private float popScaleFrom = 0.35f;
    [SerializeField] private float popDuration = 0.18f;

    [SerializeField] private TextMeshPro _textMesh;
    private Sequence _sequence;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    public void Play(int current, int maxHp, Vector3 worldPosition, Action onComplete)
    {
        transform.localScale = Vector3.one * popScaleFrom;
        transform.position = worldPosition + Vector3.up * spawnOffsetY;

        var curPct = Mathf.RoundToInt(currentHpSizePercent * 100f);
        _textMesh.fontSize = fontSizeBase;
        _textMesh.text =
            $"<size={curPct}%>{current}</size><#ffffffcc>/</color><size=100%>{maxHp}</size>";

        var c = _textMesh.color;
        c.a = 1f;
        _textMesh.color = c;

        _sequence?.Kill();
        transform.DOKill();

        var fadeDur = Mathf.Max(0.05f, floatDuration * (1f - fadeStartNormalized));
        var fadeDelay = floatDuration * fadeStartNormalized;

        _sequence = DOTween.Sequence();
        _sequence.Join(transform.DOScale(1f, popDuration).SetEase(Ease.OutBack));
        _sequence.Join(transform.DOMoveY(transform.position.y + floatUpHeight, floatDuration).SetEase(Ease.OutQuad));
        _sequence.Insert(
            fadeDelay,
            DOTween
                .To(() => _textMesh.color.a, a =>
                {
                    var col = _textMesh.color;
                    col.a = a;
                    _textMesh.color = col;
                }, 0f, fadeDur)
                .SetEase(Ease.InQuad));
        _sequence.OnComplete(() => onComplete?.Invoke());
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(
            transform.position - _camera.transform.position);
    }

    private void OnDisable()
    {
        _sequence?.Kill();
        transform.DOKill();
    }
}

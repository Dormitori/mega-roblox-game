using UnityEngine;
using DG.Tweening;

public class ArrowAnimation : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float moveAmount = 0.2f;
    [SerializeField] private float duration = 0.8f;

    private Tween _floatTween;
    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position;

        _floatTween = transform.DOMoveY(_startPos.y + moveAmount, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        _floatTween?.Kill();
    }
}
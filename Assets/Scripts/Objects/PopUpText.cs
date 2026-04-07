using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class PopUpText : MonoBehaviour
{
    public float appearTime = 0.3f;
    public float holdTime = 0.3f;
    public float disappearTime = 0.3f;
    public float upDistance = 1f;

    private TextMeshPro _text;
    private Camera _camera;
    
    public void Start()
    {
        _text = GetComponent<TextMeshPro>();
        _camera = Camera.main;
        _text.alpha = 0f;
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(transform.localPosition.y + upDistance, appearTime).SetEase(Ease.InOutElastic));
        sequence.Join(_text.DOFade(1f, appearTime));
        sequence.AppendInterval(holdTime);
        sequence.Append(_text.DOFade(0f, disappearTime));
        sequence.AppendCallback(() => Destroy(gameObject));
    }

    public void Update()
    {
        transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward,
            _camera.transform.rotation * Vector3.up);
    }
}

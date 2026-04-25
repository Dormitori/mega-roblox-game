using UnityEngine;

public class GuideLine : MonoBehaviour
{
    public Transform playerTransform;

    private Transform _target;
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        HideGuideLine();
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void ShowGuideLine()
    {
        _lineRenderer.enabled = true;
    }

    public void HideGuideLine()
    {
        _lineRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        if (!_target)
            return;
        _lineRenderer.SetPosition(0, playerTransform.position);
        _lineRenderer.SetPosition(1, _target.position);
    }
}
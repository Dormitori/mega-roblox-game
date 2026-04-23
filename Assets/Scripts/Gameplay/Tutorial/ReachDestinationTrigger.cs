using System;
using UnityEngine;

public class ReachDestinationTrigger : MonoBehaviour
{
    public event Action Reached;
    
    public GameObject floatingArrowPrefab;
    public Transform floatingArrowTransform;

    private GameObject _floatingArrow;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Reached?.Invoke();
    }

    public void ShowFloatingArrow()
    {
        _floatingArrow = Instantiate(floatingArrowPrefab, floatingArrowTransform.position, floatingArrowTransform.transform.rotation);
    }

    public void HideFloatingArrow()
    {
        if (_floatingArrow != null)
            Destroy(_floatingArrow);
    }
}
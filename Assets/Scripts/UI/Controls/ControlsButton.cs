using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControlsButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool IsPressed;
    public bool WasPressedThisFrame;

    private void LateUpdate()
    {
        WasPressedThisFrame = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
        WasPressedThisFrame = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
    }
}
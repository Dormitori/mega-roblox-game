using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PickaxeShopView : MonoBehaviour, IPointerClickHandler
{
    public event Action Clicked;

    public Image iconImage;
    public Image darkImage;
    public Image equippedToggleImage;
    public Image outlineToggleImage;

    private float _notUnlockedAlfa = 0.5f;


    public void UpdateIcon(Sprite icon)
    {
        iconImage.sprite = icon;
    }
    
    public void UpdateView(bool owned, bool equipped, bool selected)
    {
        var darkImageColor = darkImage.color;
        var equippedToggleColor = equippedToggleImage.color;
        var outlineToggleColor = outlineToggleImage.color;
        
        darkImageColor.a = !owned ? _notUnlockedAlfa : 0f;
        equippedToggleColor.a = equipped ? 1f : 0f;
        outlineToggleColor.a = selected ? 1f : 0f;
        
        darkImage.color = darkImageColor;
        equippedToggleImage.color = equippedToggleColor;
        outlineToggleImage.color = outlineToggleColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }
}
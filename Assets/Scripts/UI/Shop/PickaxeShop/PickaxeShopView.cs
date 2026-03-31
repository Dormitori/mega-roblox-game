using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PickaxeShopView : MonoBehaviour, IPointerClickHandler
{
    public event Action<PickaxeConfig> Clicked;
    
    public Image iconImage;
    public Image darkImage;
    public Image equippedToggleImage;
    public TextMeshProUGUI priceText;
    
    private PickaxeConfig _config;
    private float _notUnlockedAlfa = 0.5f;

    public void SetView(PickaxeConfig config, bool unlocked, bool equipped)
    {
        if (config != null)
        {
            _config = config;
            iconImage.sprite = config.pickaxeIcon;
            priceText.text = config.price.ToString();
        }

        var darkImageColor = darkImage.color;
        var equippedToggleColor = equippedToggleImage.color;
        
        darkImageColor.a = !unlocked ? _notUnlockedAlfa : 0f;
        equippedToggleColor.a = equipped ? 1f : 0f;
        
        darkImage.color = darkImageColor;
        equippedToggleImage.color = equippedToggleColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(_config);
    }
}
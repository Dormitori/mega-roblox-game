using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyShopEntry : MonoBehaviour
{
    public event Action<int, int> Buy;
    
    public TextMeshProUGUI resourceText;
    public TextMeshProUGUI countText;
    public Image resourceIcon;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public List<QuantityIncrementButton> quantityIncrementButtons;

    private int _chosenQuantity;
    
    private int _price;
    private int _maxCanAfford;


    public void Initialize(int entryId, Sprite icon, string resourceName, int price)
    {
        resourceIcon.sprite = icon;
        resourceText.text = $"{resourceName}";
        _price = price;
        buyButton.onClick.AddListener(() => Buy?.Invoke(entryId, _chosenQuantity));
        
        foreach (var button in quantityIncrementButtons)
        {
            button.button.onClick.AddListener(() => OnQuantityChange(button.increment));
        }
    }

    public void Refresh(int maxCanBuy)
    {
        _maxCanAfford =  maxCanBuy;
        buyButton.interactable = maxCanBuy != 0;
        _chosenQuantity = maxCanBuy == 0 ? 0 : 1;
        
        if (_chosenQuantity > maxCanBuy)
            _chosenQuantity = maxCanBuy;
        
        RefreshButtons();
        countText.text = $"x{_chosenQuantity}";
        priceText.text = $"{Mathf.Max(_chosenQuantity * _price, _price)}";
    }

    private void OnQuantityChange(int increment)
    {
        var chosenQuantity = _chosenQuantity + increment;
        _chosenQuantity = Mathf.Clamp(chosenQuantity, 1, _maxCanAfford);
        countText.text = $"x{_chosenQuantity}";
        priceText.text = $"{_chosenQuantity * _price}";
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        foreach (var button in quantityIncrementButtons)
        {
            if (button.increment < 0 && _chosenQuantity <= 1)
                button.button.interactable = false;
            else if (button.increment > 0 && _chosenQuantity >= _maxCanAfford)
                button.button.interactable = false;
            else
                button.button.interactable = true;
        }
    }
}

[Serializable]
public class QuantityIncrementButton
{
    public Button button;
    public int increment;
}
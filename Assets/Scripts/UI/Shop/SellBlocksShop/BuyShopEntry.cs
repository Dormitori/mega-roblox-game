using System;
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
    public Slider chooseQuantitySlider;

    private int _chosenQuantity;
    
    private int _price;
    private int _maxCanAfford;

    private void Awake()
    {
        chooseQuantitySlider.onValueChanged.AddListener(OnQuantityChange);
    }

    public void Initialize(int entryId, Sprite icon, string resourceName, int price)
    {
        resourceIcon.sprite = icon;
        resourceText.text = $"{resourceName}";
        _price = price;
        buyButton.onClick.AddListener(() => Buy?.Invoke(entryId, _chosenQuantity));
    }

    public void Refresh(int maxCanBuy)
    {
        _maxCanAfford =  maxCanBuy;
        
        chooseQuantitySlider.interactable = maxCanBuy > 1;
        
        buyButton.interactable = maxCanBuy != 0;

        _chosenQuantity = maxCanBuy == 0 ? 0 : 1;

        chooseQuantitySlider.value = 0;

        if (_chosenQuantity > maxCanBuy)
        {
            chooseQuantitySlider.value = 1;
            _chosenQuantity = maxCanBuy;
        }
        
        countText.text = $"x{_chosenQuantity}";
        priceText.text = $"{Mathf.Max(_chosenQuantity * _price, _price)}";
    }

    private void OnQuantityChange(float sliderValue)
    {
        var chosenQuantity = 1 + Mathf.RoundToInt(sliderValue * (_maxCanAfford - 1));
        _chosenQuantity = Mathf.Clamp(chosenQuantity, 1, _maxCanAfford);
        countText.text = $"x{chosenQuantity}";
        priceText.text = $"{chosenQuantity * _price}";
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellShopEntry : MonoBehaviour
{
    public event Action<SellShopEntry> Sell;
    
    public BlockType BlockType { get; private set; }
    public int ResourceCount { get; private set; }
    public int ResourcePrice { get; private set; }
    
    public TextMeshProUGUI resourceText;
    public TextMeshProUGUI countText;
    public Image resourceIcon;
    public TextMeshProUGUI priceText;
    public Button sellButton;
    
    public void SetResource(BlockType blockType, Sprite icon, string resourceName, int resourceCount, int resourcePrice)
    {
        BlockType = blockType;
        ResourceCount = resourceCount;
        ResourcePrice = resourcePrice;
        resourceIcon.sprite = icon;
        
        resourceText.text = $"{resourceName}";
        countText.text = $"x{resourceCount}";
        priceText.text = $"{resourcePrice * resourceCount}";
        sellButton.onClick.AddListener(() => Sell?.Invoke(this));
    }
}
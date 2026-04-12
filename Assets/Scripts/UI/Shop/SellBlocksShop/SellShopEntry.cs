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
    public int TotalSellValue { get; private set; }
    
    public TextMeshProUGUI resourceText;
    public TextMeshProUGUI countText;
    public Image resourceIcon;
    public TextMeshProUGUI priceText;
    public Button sellButton;
    
    public void SetResource(BlockType blockType, Sprite icon, string resourceName, int resourceCount, int totalSellValue)
    {
        BlockType = blockType;
        ResourceCount = resourceCount;
        TotalSellValue = totalSellValue;
        ResourcePrice = resourceCount > 0 ? Mathf.Max(1, totalSellValue / resourceCount) : 0;
        resourceIcon.sprite = icon;
        
        resourceText.text = $"{resourceName}";
        countText.text = $"x{resourceCount}";
        priceText.text = $"{totalSellValue}";
        sellButton.onClick.AddListener(() => Sell?.Invoke(this));
    }
}
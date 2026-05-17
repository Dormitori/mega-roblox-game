using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EggShopEggView : MonoBehaviour
{
    public event Action BuyCoins;
    public event Action BuyCrystals;
    public event Action Hatch;

    public Image iconImage;
    public TextMeshProUGUI titleText;

    public Image rowPlateBackground;
    public Sprite plateBackgroundSimple;
    public Sprite plateBackgroundRare;
    public Sprite plateBackgroundEpic;

    public Button buyCoinsButton;
    public TextMeshProUGUI buyCoinsPriceText;
    public Button buyCrystalsButton;
    public TextMeshProUGUI buyCrystalsPriceText;

    [Header("Inventory / Hatch")]
    public TextMeshProUGUI inventoryCountText;
    public Button hatchButton;

    private EggConfig _cfg;

    private void Awake()
    {
        if (buyCoinsButton != null)
            buyCoinsButton.onClick.AddListener(() => BuyCoins?.Invoke());
        if (buyCrystalsButton != null)
            buyCrystalsButton.onClick.AddListener(() => BuyCrystals?.Invoke());
        if (hatchButton != null)
            hatchButton.onClick.AddListener(() => Hatch?.Invoke());
    }

    public void Bind(EggConfig cfg)
    {
        _cfg = cfg;
        if (_cfg == null) return;

        if (iconImage != null) iconImage.sprite = _cfg.icon;
        if (titleText != null) titleText.text = _cfg.LocalizedName;
        ApplyRowPlate();
        if (buyCoinsPriceText != null) buyCoinsPriceText.text = _cfg.priceCoins.ToString();
        if (buyCrystalsPriceText != null) buyCrystalsPriceText.text = _cfg.priceCrystals.ToString();
    }

    public void SetInteractable(bool coins, bool crystals)
    {
        if (buyCoinsButton != null) buyCoinsButton.interactable = coins;
        if (buyCrystalsButton != null) buyCrystalsButton.interactable = crystals;
    }

    public void SetHatchState(int inventoryCount, bool slotAvailable)
    {
        if (inventoryCountText != null)
            inventoryCountText.text = inventoryCount > 0 ? $"x{inventoryCount}" : string.Empty;
        if (hatchButton != null)
            hatchButton.interactable = inventoryCount > 0 && slotAvailable;
    }

    private void ApplyRowPlate()
    {
        if (rowPlateBackground == null) return;
        var s = PlateSpriteForEggId(_cfg.id);
        if (s != null) rowPlateBackground.sprite = s;
    }

    private Sprite PlateSpriteForEggId(string eggId)
    {
        return eggId switch
        {
            "Rare" => plateBackgroundRare != null ? plateBackgroundRare : plateBackgroundSimple,
            "Epic" => plateBackgroundEpic != null ? plateBackgroundEpic : plateBackgroundSimple,
            _ => plateBackgroundSimple
        };
    }
}


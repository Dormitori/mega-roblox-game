using System.Collections.Generic;
using System.Linq;
using Core.Audio;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SellShop : PopUpWindow
{
    public SellShopEntry shopEntryPrefab;
    public Transform shopEntriesTransform;
    public Button sellAllButton;
    public TutorialManager tutorialManager;

    private List<BlockConfig> _blockConfigs;
    private Inventory _inventory;
    private IAudioService _audioService;
    private List<SellShopEntry> _shopEntries = new();

    public override void Awake()
    {
        base.Awake();
        sellAllButton.onClick.AddListener(SellAll);
    }

    [Inject]
    public void Initialize(Inventory inventory, ConfigManager<BlockConfig> configManager, IAudioService audioService)
    {
        _inventory = inventory;
        _blockConfigs = configManager.Configs;
        _audioService = audioService;
    }

    public override void OnWindowShow()
    {
        base.OnWindowShow();
        foreach (var blockConfig in _blockConfigs)
        {
            if (_inventory.GetBlockCount(blockConfig.type) == 0)
                continue;
            
            var shopEntry = Instantiate(shopEntryPrefab, Vector3.zero, Quaternion.identity, shopEntriesTransform);

            var count = _inventory.GetBlockCount(blockConfig.type);
            var total = (int)Mathf.Min(int.MaxValue, _inventory.GetSellValueTotal(blockConfig.type));

            var sellEnabled = true;
            if (!tutorialManager.TutorialIsCompleted && new[] {BlockType.Ground, BlockType.Stone}.Contains(blockConfig.type))
                sellEnabled = false;
            
            sellAllButton.interactable = sellEnabled;
            shopEntry.SetResource(
                blockConfig.type,
                blockConfig.icon,
                blockConfig.LocalizedName,
                count,
                total,
                sellEnabled
            );
            _shopEntries.Add(shopEntry);
            shopEntry.Sell += OnSell;
        }
    }

    private void OnSell(SellShopEntry shopEntry)
    {
        _inventory.TryRemoveBlock(shopEntry.BlockType, shopEntry.ResourceCount);
        _inventory.AddCurrency(CurrencyType.Coins, shopEntry.TotalSellValue);
        _audioService?.PlaySfx(SoundId.BuySell);
        shopEntry.Sell -= OnSell;
        _shopEntries.Remove(shopEntry);
        Destroy(shopEntry.gameObject);
    }

    private void SellAll()
    {
        if (_shopEntries.Count == 0)
            return;

        foreach (var shopEntry in _shopEntries)
        {
            _inventory.TryRemoveBlock(shopEntry.BlockType, shopEntry.ResourceCount);
            _inventory.AddCurrency(CurrencyType.Coins, shopEntry.TotalSellValue);
            shopEntry.Sell -= OnSell;
            Destroy(shopEntry.gameObject);
        }

        _shopEntries.Clear();
        _audioService?.PlaySfx(SoundId.BuySell);
    }

    public override void OnWindowHide()
    {
        base.OnWindowHide();
        foreach (var shopEntry in _shopEntries)
            Destroy(shopEntry.gameObject);
        _shopEntries.Clear();
    }
}
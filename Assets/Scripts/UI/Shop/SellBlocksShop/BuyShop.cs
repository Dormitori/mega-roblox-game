using System.Collections.Generic;
using System.Linq;
using Core.Audio;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class BuyShop : PopUpWindow
{
    public BuyShopEntry shopEntryPrefab;
    public Transform shopEntriesTransform;

    private List<BlockConfig> _blockConfigs;
    private Inventory _inventory;
    private IAudioService _audioService;
    private List<(BuyShopEntry, BlockConfig)> _shopEntries = new();


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
        Clear();
        var i = 0;
        foreach (var unlockedBuyableBlock in _inventory.UnlockedBuyableBlocks)
        {
            var shopEntry = Instantiate(shopEntryPrefab, shopEntriesTransform);
            var config = _blockConfigs.FirstOrDefault(x => x.type == unlockedBuyableBlock);
            shopEntry.Initialize(i, config.icon, config.LocalizedName, config.BuyPrice);
            _shopEntries.Add((shopEntry, config));
            shopEntry.Buy += OnBuy;
            i++;
        }
        Refresh();
    }

    private void OnBuy(int idx, int quantity)
    {
        var (_, config) = _shopEntries[idx];
        if (_inventory.TryRemoveCurrency(CurrencyType.Coins, quantity * config.BuyPrice))
            _inventory.AddBlock(config.type, quantity, config.baseSellPrice);
        
        Refresh();
    }

    private void Clear()
    {
        foreach (var (entry, _) in _shopEntries) {
            entry.Buy -= OnBuy;
            Destroy(entry.gameObject);
        }
        _shopEntries.Clear();
    }

    private void Refresh()
    {
        foreach (var (entry, config) in _shopEntries)
        {
            var maxCanBuy = Mathf.Min(_inventory.GetSpaceLeft(),
                _inventory.GetCurrencyCount(CurrencyType.Coins) / config.BuyPrice);
            entry.Refresh(maxCanBuy);
        }
    }
}
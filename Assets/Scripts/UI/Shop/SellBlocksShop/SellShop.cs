using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SellShop : PopUpWindow
{
    public SellShopEntry shopEntryPrefab;
    public Transform shopEntriesTransform;
    public Button sellAllButton;

    private List<BlockConfig> _blockConfigs;
    private Inventory _inventory;
    private List<SellShopEntry> _shopEntries = new();

    public override void Awake()
    {
        base.Awake();
        sellAllButton.onClick.AddListener(SellAll);
    }

    [Inject]
    public void Initialize(Inventory inventory, ConfigManager<BlockConfig> configManager)
    {
        _inventory = inventory;
        _blockConfigs = configManager.Configs;
    }

    public override void OnWindowShow()
    {
        base.OnWindowShow();
        foreach (var blockConfig in _blockConfigs)
        {
            if (_inventory.GetBlockCount(blockConfig.type) == 0)
                continue;
            
            var shopEntry = Instantiate(shopEntryPrefab, Vector3.zero, Quaternion.identity, shopEntriesTransform);

            shopEntry.SetResource(
                blockConfig.type,
                blockConfig.icon,
                blockConfig.name,
                _inventory.GetBlockCount(blockConfig.type),
                blockConfig.baseSellPrice
            );
            _shopEntries.Add(shopEntry);
            shopEntry.Sell += OnSell;
        }
    }

    private void OnSell(SellShopEntry shopEntry)
    {
        _inventory.TryRemoveBlock(shopEntry.BlockType, shopEntry.ResourceCount);
        _inventory.AddCurrency(CurrencyType.Coins, shopEntry.ResourceCount * shopEntry.ResourcePrice);
        shopEntry.Sell -= OnSell;
        _shopEntries.Remove(shopEntry);
        Destroy(shopEntry.gameObject);
    }

    private void SellAll()
    {
        foreach (var shopEntry in _shopEntries)
        {
            _inventory.TryRemoveBlock(shopEntry.BlockType, shopEntry.ResourceCount);
            _inventory.AddCurrency(CurrencyType.Coins, shopEntry.ResourceCount * shopEntry.ResourcePrice);
            shopEntry.Sell -= OnSell;
            Destroy(shopEntry.gameObject);
        }

        _shopEntries.Clear();
    }

    public override void OnWindowHide()
    {
        base.OnWindowHide();
        foreach (var shopEntry in _shopEntries)
            Destroy(shopEntry.gameObject);
        _shopEntries.Clear();
    }
}
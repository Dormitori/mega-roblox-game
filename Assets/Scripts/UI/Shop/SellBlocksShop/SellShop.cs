using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SellShop : PopUpWindow
{
    public List<BlockConfig> blockConfigs;
    public SellShopEntry shopEntryPrefab;
    public Transform shopEntriesTransform;
    public Button sellAllButton;
    
    private IInventory _inventory;
    private List<SellShopEntry> _shopEntries = new();

    public override void Awake()
    {
        base.Awake();
        sellAllButton.onClick.AddListener(SellAll);
    }

    [Inject]
    public void Initialize(IInventory inventory)
    {
        _inventory = inventory;
    }

    public override void OnWindowShow()
    {
        base.OnWindowShow();
        foreach (BlockConfig blockConfig in blockConfigs)
        foreach (var item in _inventory.ItemsCount.Keys)
        {
            if (blockConfig.item == item && _inventory.GetItemCount(item) > 0)
            {
                var shopEntry = Instantiate(shopEntryPrefab, Vector3.zero, Quaternion.identity, shopEntriesTransform);
                shopEntry.SetResource(
                    blockConfig.item, 
                    blockConfig.icon,
                    blockConfig.name, 
                    _inventory.ItemsCount[item], 
                    blockConfig.baseSellPrice
                    );
                _shopEntries.Add(shopEntry);
                shopEntry.Sell += OnSell;
            }
        }
    }

    private void OnSell(SellShopEntry shopEntry)
    {
        _inventory.TryRemoveItem(shopEntry.Item, shopEntry.ResourceCount);
        _inventory.AddItem(Items.Coins, shopEntry.ResourceCount * shopEntry.ResourcePrice);
        shopEntry.Sell -= OnSell;
        _shopEntries.Remove(shopEntry);
        Destroy(shopEntry.gameObject);
    }

    private void SellAll()
    {
        foreach (var shopEntry in _shopEntries)
        {
            _inventory.TryRemoveItem(shopEntry.Item, shopEntry.ResourceCount);
            _inventory.AddItem(Items.Coins, shopEntry.ResourceCount * shopEntry.ResourcePrice);
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
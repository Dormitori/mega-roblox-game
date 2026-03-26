using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SellShop : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    public InputControls characterControls;
    public List<BlockConfig> blockConfigs;
    public SellShopEntry shopEntryPrefab;
    public Transform shopEntriesTransform;
    public Button sellAllButton;
    public Button closeButton;
    
    private IInventory _inventory;
    private List<SellShopEntry> _shopEntries = new();

    public void Awake()
    {
        HidePanel();
        sellAllButton.onClick.AddListener(SellAll);
        closeButton.onClick.AddListener(CloseShop);
    }

    [Inject]
    public void Initialize(IInventory inventory)
    {
        _inventory = inventory;
    }

    public void OpenShop()
    {
        if (_canvasGroup.alpha == 0)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1;
        }
        characterControls.CanMine = false;
        characterControls.CanCameraScroll = false;
        
        foreach (BlockConfig blockConfig in blockConfigs)
        foreach (var item in _inventory.Items.Keys)
        {
            if (blockConfig.item == item && _inventory.GetItemCount(item) > 0)
            {
                var shopEntry = Instantiate(shopEntryPrefab, Vector3.zero, Quaternion.identity, shopEntriesTransform);
                shopEntry.SetResource(
                    blockConfig.item, 
                    blockConfig.icon,
                    blockConfig.name, 
                    _inventory.Items[item], 
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

    public void CloseShop()
    {
        characterControls.CanMine = true;
        characterControls.CanCameraScroll = true;
        foreach (var shopEntry in _shopEntries)
            Destroy(shopEntry.gameObject);
        _shopEntries.Clear();
        HidePanel();
    }
    
    private void HidePanel()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
    }
    
}
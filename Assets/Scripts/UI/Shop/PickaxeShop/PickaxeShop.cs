using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PickaxeShop : PopUpWindow
{
    public List<PickaxeConfig> pickaxeConfigs;
    public Transform shopItemsTransform;
    public PlayerPickaxe playerPickaxe;
    public PickaxeShopView pickaxeShopViewPrefab;
    public SelectedPickaxeView selectedPickaxeView;

    private IInventory _inventory;
    private PickaxeShopView _equippedPickaxeView;
    private PickaxeShopView _selectedPickaxeView;

    [Inject]
    public void Initialize(IInventory inventory)
    {
        _inventory = inventory;
    }

    public override void Awake()
    {
        base.Awake();
        
        // Проверки на null
        if (selectedPickaxeView == null)
        {
            Debug.LogError("PickaxeShop: selectedPickaxeView is not assigned!");
            return;
        }
        
        if (pickaxeShopViewPrefab == null)
        {
            Debug.LogError("PickaxeShop: pickaxeShopViewPrefab is not assigned!");
            return;
        }
        
        if (shopItemsTransform == null)
        {
            Debug.LogError("PickaxeShop: shopItemsTransform is not assigned!");
            return;
        }
        
        InitShop();
        selectedPickaxeView.Buy += OnBuy;
        selectedPickaxeView.Equip += OnEquip;
    }

    private void InitShop()
    {
        if (pickaxeConfigs == null || pickaxeConfigs.Count == 0)
        {
            Debug.LogError("PickaxeShop: pickaxeConfigs is null or empty!");
            return;
        }
        
        if (_inventory == null)
        {
            Debug.LogError("PickaxeShop: _inventory is null! Make sure Initialize() was called.");
            return;
        }
        
        foreach (var pickaxeConfig in pickaxeConfigs)
        {
            if (pickaxeConfig == null)
            {
                Debug.LogWarning("PickaxeShop: Found null pickaxeConfig in list, skipping.");
                continue;
            }
            
            var view = Instantiate(pickaxeShopViewPrefab, shopItemsTransform);
            var hasItem = _inventory.ItemsCount[pickaxeConfig.item] > 0;
            var equipped = playerPickaxe.CurrentPickaxeConfig.item == pickaxeConfig.item;
            if (equipped)
            {
                _equippedPickaxeView = view;
                selectedPickaxeView.SetSelected(pickaxeConfig, _equippedPickaxeView, true, true);
                _selectedPickaxeView = view; // Equipped pickaxe is also selected by default
            }

            view.SetView(pickaxeConfig, hasItem, equipped, equipped); // Selected if equipped
            view.Clicked += config =>
            {
                // Update selection for all views
                if (_selectedPickaxeView && _selectedPickaxeView != view)
                {
                    var previousConfig = GetConfigForView(_selectedPickaxeView);
                    if (previousConfig != null)
                    {
                        var previousUnlocked = _inventory.ItemsCount[previousConfig.item] > 0;
                        var previousEquipped = playerPickaxe.CurrentPickaxeConfig.item == previousConfig.item;
                        _selectedPickaxeView.SetView(previousConfig, previousUnlocked, previousEquipped, false); // Deselect previous
                    }
                }
                _selectedPickaxeView = view;
                
                // Update current selection
                view.SetView(config, _inventory.ItemsCount[config.item] > 0, 
                           playerPickaxe.CurrentPickaxeConfig.item == config.item, true); // Select current
                
                selectedPickaxeView.SetSelected(
                    config,
                    view,
                    playerPickaxe.CurrentPickaxeConfig.item == config.item,
                    _inventory.ItemsCount[config.item] > 0,
                    _inventory.ItemsCount[Items.Coins] >= config.price
                );
            };
        }
    }

    private PickaxeConfig GetConfigForView(PickaxeShopView view)
    {
        if (view == null || view.iconImage == null)
        {
            Debug.LogError("GetConfigForView: view or iconImage is null");
            return null;
        }
        
        var config = pickaxeConfigs.Find(c => c.pickaxeIcon == view.iconImage.sprite);
        if (config == null)
        {
            Debug.LogError($"GetConfigForView: No config found for sprite {view.iconImage.sprite?.name}");
        }
        
        return config;
    }

    private void OnBuy(PickaxeConfig config, PickaxeShopView view)
    {
        if (!_inventory.TryRemoveItem(Items.Coins, config.price))
            return;

        _inventory.AddItem(config.item, 1);
        selectedPickaxeView.SetSelected(config, view, false, true);
        view.SetView(config, true, false, true); // Selected after buy
    }

    private void OnEquip(PickaxeConfig config, PickaxeShopView view)
    {
        selectedPickaxeView.SetSelected(config, view, true, true);
        view.SetView(config, true, true, true); // Selected and equipped

        if (_equippedPickaxeView && _equippedPickaxeView != view)
        {
            // ✅ Получаем конфиг СТАРОЙ экипированной кирки
            var previousConfig = GetConfigForView(_equippedPickaxeView);
            if (previousConfig != null)
            {
                var previousUnlocked = _inventory.ItemsCount[previousConfig.item] > 0;
                // ✅ Обновляем СТАРУЮ кирку с ЕЕ конфигом
                _equippedPickaxeView.SetView(previousConfig, previousUnlocked, false, false);
            }
        }

        _equippedPickaxeView = view;
        _selectedPickaxeView = view;
        playerPickaxe.EquipPickaxe(config);
    }
}
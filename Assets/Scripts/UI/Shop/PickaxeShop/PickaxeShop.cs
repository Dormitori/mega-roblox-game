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

    [Inject]
    public void Initialize(IInventory inventory)
    {
        _inventory = inventory;
    }

    public override void Awake()
    {
        base.Awake();
        InitShop();
        selectedPickaxeView.Buy += OnBuy;
        selectedPickaxeView.Equip += OnEquip;
    }

    private void InitShop()
    {
        foreach (var pickaxeConfig in pickaxeConfigs)
        {
            var view = Instantiate(pickaxeShopViewPrefab, shopItemsTransform);
            var hasItem = _inventory.ItemsCount[pickaxeConfig.item] > 0;
            var equipped = playerPickaxe.CurrentPickaxeConfig.item == pickaxeConfig.item;
            if (equipped)
            {
                _equippedPickaxeView = view;
                selectedPickaxeView.SetSelected(pickaxeConfig, _equippedPickaxeView, true, true);
            }

            view.SetView(pickaxeConfig, hasItem, equipped);
            view.Clicked += config =>
            {
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

    private void OnBuy(PickaxeConfig config, PickaxeShopView view)
    {
        if (!_inventory.TryRemoveItem(Items.Coins, config.price))
            return;

        _inventory.AddItem(config.item, 1);
        selectedPickaxeView.SetSelected(config, view, false, true);
        view.SetView(config, true, false);
    }

    private void OnEquip(PickaxeConfig config, PickaxeShopView view)
    {
        selectedPickaxeView.SetSelected(config, view, true, true);
        view.SetView(config, true, true);

        if (_equippedPickaxeView)
            _equippedPickaxeView.SetView(null, true, false);

        _equippedPickaxeView = view;
        playerPickaxe.EquipPickaxe(config);
    }
}
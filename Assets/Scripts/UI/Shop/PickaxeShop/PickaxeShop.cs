using System.Collections.Generic;
using UnityEngine;
using VContainer;
public class PickaxeShop : PopUpWindow
{
    public Transform shopItemsTransform;
    public PlayerPickaxe playerPickaxe;
    public PickaxeShopView pickaxeShopViewPrefab;
    public SelectedPickaxeView selectedPickaxeView;

    private List<PickaxeConfig> _pickaxeConfigs;
    private List<BlockConfig> _blockConfigs;
    private IInventory _inventory;

    private List<(PickaxeShopView, PickaxeConfig)> _pickaxeViews = new();

    // state
    private PickaxeConfig _equippedPickaxe;
    private PickaxeConfig _selectedPickaxe;

    [Inject]
    public void Initialize(
        IInventory inventory,
        ConfigManager<BlockConfig> blockConfigs,
        ConfigManager<PickaxeConfig> pickaxeConfigs
    )
    {
        _inventory = inventory;
        _pickaxeConfigs = pickaxeConfigs.Configs;
        _blockConfigs = blockConfigs.Configs;
        InitShop();
    }

    public override void Awake()
    {
        base.Awake();

        selectedPickaxeView.Buy += OnBuy;
        selectedPickaxeView.Equip += OnEquip;
    }

    private void Refresh()
    {
        foreach (var (view, config) in _pickaxeViews)
        {
            view.UpdateView(
                _inventory.HasItem(config.item),
                config == _equippedPickaxe,
                config == _selectedPickaxe
            );
        }

        selectedPickaxeView.UpdateView(
            _selectedPickaxe,
            _inventory.HasItem(_selectedPickaxe.item),
            _selectedPickaxe == _equippedPickaxe
        );
    }

    private void InitShop()
    {
        selectedPickaxeView.Initialize(_blockConfigs, _inventory);
        _selectedPickaxe = playerPickaxe.defaultPickaxeConfig;
        _equippedPickaxe = playerPickaxe.defaultPickaxeConfig;

        foreach (var pickaxeConfig in _pickaxeConfigs)
        {
            var view = Instantiate(pickaxeShopViewPrefab, shopItemsTransform);
            view.UpdateIcon(pickaxeConfig.pickaxeIcon);
            _pickaxeViews.Add((view, pickaxeConfig));

            view.Clicked += () => {
                _selectedPickaxe = pickaxeConfig;
                Refresh();
            };
        }
    }

    public override void OnWindowShow()
    {
        base.OnWindowShow();
        Refresh();
    }


    private void OnBuy()
    {
        if (!_selectedPickaxe.TryBuy(_inventory))
            return;

        _inventory.AddItem(_selectedPickaxe.item, 1);
        Refresh();
    }

    private void OnEquip()
    {
        _equippedPickaxe = _selectedPickaxe;
        playerPickaxe.EquipPickaxe(_selectedPickaxe);
        Refresh();
    }
}
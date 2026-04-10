using System.Collections.Generic;
using Core.Audio;
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
    private Inventory _inventory;
    private IAudioService _audioService;

    private List<(PickaxeShopView, PickaxeConfig)> _pickaxeViews = new();

    // state
    private PickaxeConfig _equippedPickaxe;
    private PickaxeConfig _selectedPickaxe;

    [Inject]
    public void Initialize(
        Inventory inventory,
        ConfigManager<BlockConfig> blockConfigs,
        ConfigManager<PickaxeConfig> pickaxeConfigs,
        IAudioService audioService
    )
    {
        _inventory = inventory;
        _pickaxeConfigs = pickaxeConfigs.Configs;
        _blockConfigs = blockConfigs.Configs;
        _audioService = audioService;
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
                _inventory.HasPickaxe(config.pickaxeType),
                config == _equippedPickaxe,
                config == _selectedPickaxe
            );
        }

        selectedPickaxeView.UpdateView(
            _selectedPickaxe,
            _inventory.HasPickaxe(_selectedPickaxe.pickaxeType),
            _selectedPickaxe == _equippedPickaxe
        );
    }

    private void InitShop()
    {
        selectedPickaxeView.Initialize(_blockConfigs, _inventory);
        _selectedPickaxe = playerPickaxe.defaultPickaxeConfig;
        _equippedPickaxe = playerPickaxe.defaultPickaxeConfig;
        _inventory.AddPickaxe(playerPickaxe.defaultPickaxeConfig.pickaxeType);

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

        _inventory.AddPickaxe(_selectedPickaxe.pickaxeType);
        _audioService?.PlaySfx(SoundId.BuySell);
        Refresh();
    }

    private void OnEquip()
    {
        _equippedPickaxe = _selectedPickaxe;
        playerPickaxe.EquipPickaxe(_selectedPickaxe);
        Refresh();
    }
}
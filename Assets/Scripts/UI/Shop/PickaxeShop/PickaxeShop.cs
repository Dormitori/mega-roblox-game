using System.Collections.Generic;
using Core.Audio;
using UnityEngine;
using VContainer;

public class PickaxeShop : PopUpWindow
{
    public Transform shopItemsTransform;
    public PickaxeShopView pickaxeShopViewPrefab;
    public SelectedPickaxeView selectedPickaxeView;

    private List<PickaxeConfig> _pickaxeConfigs;
    private List<BlockConfig> _blockConfigs;
    private Inventory _inventory;
    private IAudioService _audioService;
    private PlayerPickaxe _playerPickaxe;

    private List<(PickaxeShopView, PickaxeConfig)> _pickaxeViews = new();

    // state
    private PickaxeConfig _equippedPickaxe;
    private PickaxeConfig _selectedPickaxe;

    [Inject]
    public void Initialize(
        Inventory inventory,
        PlayerPickaxe playerPickaxe,
        ConfigManager<BlockConfig> blockConfigs,
        ConfigManager<PickaxeConfig> pickaxeConfigs,
        IAudioService audioService
    )
    {
        _inventory = inventory;
        _playerPickaxe = playerPickaxe;
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
                _inventory.HasPickaxe(config.type),
                config == _equippedPickaxe,
                config == _selectedPickaxe
            );
        }

        selectedPickaxeView.UpdateView(
            _selectedPickaxe,
            _inventory.HasPickaxe(_selectedPickaxe.type),
            _selectedPickaxe == _equippedPickaxe
        );
    }

    private void InitShop()
    {
        selectedPickaxeView.Initialize(_blockConfigs, _inventory);
        _selectedPickaxe = _playerPickaxe.CurrentPickaxeConfig;
        _equippedPickaxe = _playerPickaxe.CurrentPickaxeConfig;

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

        _inventory.AddPickaxe(_selectedPickaxe.type);
        _audioService?.PlaySfx(SoundId.BuySell);
        Refresh();
    }

    private void OnEquip()
    {
        _equippedPickaxe = _selectedPickaxe;
        _playerPickaxe.EquipPickaxe(_selectedPickaxe.type);
        Refresh();
    }
}
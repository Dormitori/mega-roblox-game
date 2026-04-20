using System.Collections.Generic;
using System.Linq;
using Core.Audio;
using UnityEngine;
using VContainer;

public class EggShop : PopUpWindow
{
    [Header("Eggs (left)")]
    public Transform eggButtonsRoot;
    public EggShopEggView eggViewPrefab;

    [Header("Pets (center)")]
    public Transform petGridRoot;
    public PetShopGridItemView petItemViewPrefab;

    [Header("Selected (right)")]
    public SelectedPetView selectedPetView;

    private Inventory _inventory;
    private IAudioService _audio;
    private EggIncubatorService _incubator;
    private PetProgressService _petProgress;
    private PetEquipService _petEquip;
    private List<EggConfig> _eggConfigs;
    private List<PetConfig> _petConfigs;

    private readonly List<(EggShopEggView view, EggConfig cfg)> _eggViews = new();
    private readonly List<(PetShopGridItemView view, PetConfig cfg)> _petViews = new();

    private PetConfig _selectedPet;

    [Inject]
    private void Initialize(
        Inventory inventory,
        EggIncubatorService incubator,
        PetProgressService petProgress,
        PetEquipService petEquip,
        ConfigManager<EggConfig> eggConfigs,
        ConfigManager<PetConfig> petConfigs,
        IAudioService audioService)
    {
        _inventory = inventory;
        _incubator = incubator;
        _petProgress = petProgress;
        _petEquip = petEquip;
        _audio = audioService;
        _eggConfigs = eggConfigs.Configs;
        _petConfigs = petConfigs.Configs;

        BuildOnce();
        SubscribeEvents();
    }

    public override void Awake()
    {
        base.Awake();
    }

    private void SubscribeEvents()
    {
        UnsubscribeEvents();
        if (_inventory != null) _inventory.CurrencyChanged += Refresh;
        if (_petProgress != null) _petProgress.Changed += Refresh;
    }

    private void UnsubscribeEvents()
    {
        if (_inventory != null) _inventory.CurrencyChanged -= Refresh;
        if (_petProgress != null) _petProgress.Changed -= Refresh;
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    public override void OnWindowShow()
    {
        base.OnWindowShow();
        Refresh();
    }

    private void BuildOnce()
    {
        if (eggButtonsRoot != null && eggViewPrefab != null && _eggViews.Count == 0)
        {
            foreach (var egg in _eggConfigs.Where(e => e != null))
            {
                var view = Instantiate(eggViewPrefab, eggButtonsRoot);
                view.Bind(egg);
                view.BuyCoins += () => TryBuyEgg(egg, CurrencyType.Coins);
                view.BuyCrystals += () => TryBuyEgg(egg, CurrencyType.Crystals);
                _eggViews.Add((view, egg));
            }
        }

        if (petGridRoot != null && petItemViewPrefab != null && _petViews.Count == 0)
        {
            foreach (var pet in _petConfigs.Where(p => p != null))
            {
                var view = Instantiate(petItemViewPrefab, petGridRoot);
                view.SetIcon(pet.icon);
                view.Clicked += () =>
                {
                    _selectedPet = pet;
                    Refresh();
                };
                _petViews.Add((view, pet));
            }
        }

        if (selectedPetView != null)
        {
            selectedPetView.Take += () =>
            {
                if (_selectedPet == null) return;
                if (_petEquip.TryEquip(_selectedPet.id))
                    _audio?.PlaySfx(SoundId.BuySell);
                Refresh();
            };
        }

        _selectedPet ??= _petProgress?.GetEquippedPetConfig() ?? _petConfigs.FirstOrDefault();
    }

    private void TryBuyEgg(EggConfig egg, CurrencyType currency)
    {
        if (_incubator == null || egg == null) return;
        if (_incubator.TryPurchaseEgg(egg, currency))
        {
            _audio?.PlaySfx(SoundId.BuySell);
            Refresh();
        }
    }

    private void Refresh()
    {
        if (_inventory == null || _incubator == null || _petProgress == null)
            return;

        var equippedId = _petProgress.EquippedPetId;

        foreach (var (view, cfg) in _eggViews)
        {
            var canCoins = _inventory.GetCurrencyCount(CurrencyType.Coins) >= cfg.priceCoins;
            var canCrystals = _inventory.GetCurrencyCount(CurrencyType.Crystals) >= cfg.priceCrystals;
            view.SetInteractable(canCoins, canCrystals);
        }

        foreach (var (view, cfg) in _petViews)
        {
            var owned = _petProgress.GetOwnedCount(cfg.id);
            view.UpdateState(
                ownedCount: owned,
                equipped: cfg.id == equippedId,
                selected: _selectedPet != null && cfg.id == _selectedPet.id
            );
        }

        if (selectedPetView != null)
        {
            var ownedCount = _selectedPet != null ? _petProgress.GetOwnedCount(_selectedPet.id) : 0;
            var equipped = _selectedPet != null && _selectedPet.id == equippedId;
            selectedPetView.UpdateView(_selectedPet, ownedCount, equipped);
        }
    }
}


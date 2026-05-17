using System.Collections.Generic;
using System.Linq;
using Core.Audio;
using UnityEngine;
using VContainer;

public class PetShop : PopUpWindow
{
    [Header("Pets (center)")]
    public Transform petGridRoot;
    public PetShopGridItemView petItemViewPrefab;

    [Header("Selected (right)")]
    public SelectedPetView selectedPetView;

    private Inventory _inventory;
    private IAudioService _audio;
    private PetProgressService _petProgress;
    private PetEquipService _petEquip;
    private List<PetConfig> _petConfigs;

    private readonly List<(PetShopGridItemView view, PetConfig cfg)> _petViews = new();

    private PetConfig _selectedPet;

    [Inject]
    private void Initialize(
        Inventory inventory,
        PetProgressService petProgress,
        PetEquipService petEquip,
        ConfigManager<PetConfig> petConfigs,
        IAudioService audioService)
    {
        _inventory = inventory;
        _petProgress = petProgress;
        _petEquip = petEquip;
        _audio = audioService;
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

    private void Refresh()
    {
        if (_inventory == null || _petProgress == null)
            return;

        var equippedId = _petProgress.EquippedPetId;

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

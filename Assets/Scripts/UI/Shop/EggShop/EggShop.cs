using System.Collections.Generic;
using System.Linq;
using Core.Audio;
using UnityEngine;
using VContainer;

public class EggShop : PopUpWindow
{
    [Header("Eggs")]
    public Transform eggButtonsRoot;
    public EggShopEggView eggViewPrefab;

    [Header("Hatch Slots")]
    public EggHatchSlotView[] hatchSlots = new EggHatchSlotView[3];

    [Header("Pet Reveal")]
    public PetRevealPopup petRevealPopup;

    private Inventory _inventory;
    private IAudioService _audio;
    private List<EggConfig> _eggConfigs;
    private EggHatchingService _hatchingService;

    private readonly List<(EggShopEggView view, EggConfig cfg)> _eggViews = new();

    [Inject]
    private void Initialize(
        Inventory inventory,
        ConfigManager<EggConfig> eggConfigs,
        IAudioService audioService,
        EggHatchingService hatchingService)
    {
        _inventory = inventory;
        _audio = audioService;
        _eggConfigs = eggConfigs.Configs;
        _hatchingService = hatchingService;

        BuildOnce();
        InitHatchSlots();
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        UnsubscribeEvents();
        if (_inventory != null)
        {
            _inventory.CurrencyChanged += OnCurrencyChanged;
            _inventory.EggsChanged += RefreshEggViews;
        }
        if (_hatchingService != null) _hatchingService.SlotsChanged += OnSlotsChanged;
    }

    private void UnsubscribeEvents()
    {
        if (_inventory != null)
        {
            _inventory.CurrencyChanged -= OnCurrencyChanged;
            _inventory.EggsChanged -= RefreshEggViews;
        }
        if (_hatchingService != null) _hatchingService.SlotsChanged -= OnSlotsChanged;
    }

    // Slot changes affect both slot views and hatch button availability.
    private void OnSlotsChanged()
    {
        RefreshSlots();
        RefreshEggViews();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    public override void OnWindowShow()
    {
        base.OnWindowShow();
        Refresh();
        RefreshSlots();
        RefreshEggViews();
    }

    private void BuildOnce()
    {
        if (eggButtonsRoot != null && eggViewPrefab != null && _eggViews.Count == 0)
        {
            ClearEggsViews();
            foreach (var egg in _eggConfigs.Where(e => e != null).Reverse())
            {
                var view = Instantiate(eggViewPrefab, eggButtonsRoot);
                view.Bind(egg);
                view.BuyCoins += () => TryBuyEgg(egg, CurrencyType.Coins);
                view.BuyCrystals += () => TryBuyEgg(egg, CurrencyType.Crystals);
                view.Hatch += () => TryHatchEgg(egg);
                _eggViews.Add((view, egg));
            }
        }
    }

    private void ClearEggsViews()
    {
        if (eggButtonsRoot == null) return;

        foreach (Transform child in eggButtonsRoot.transform)
            Destroy(child.gameObject);
    }

    private void InitHatchSlots()
    {
        for (int i = 0; i < hatchSlots.Length; i++)
        {
            if (hatchSlots[i] == null) continue;
            hatchSlots[i].SetEmpty();

            int slotIndex = i;
            hatchSlots[i].OnWatchAd  += () => OnSlotWatchAd(slotIndex);
            hatchSlots[i].OnHatchNow += () => OnSlotHatchNow(slotIndex);
            hatchSlots[i].OnOpen     += () => OnSlotOpen(slotIndex);
        }
    }

    private void OnSlotOpen(int slotIndex)
    {
        var cfg = _hatchingService?.GetSlotConfig(slotIndex);
        if (cfg == null || petRevealPopup == null) return;

        HideWindowAnimated();
        petRevealPopup.Setup(cfg, slotIndex);
        petRevealPopup.ShowWindowAnimated();
    }

    private void OnSlotWatchAd(int slotIndex) => _hatchingService?.SkipByAd(slotIndex);

    private void OnSlotHatchNow(int slotIndex)
    {
        var cfg = _hatchingService?.GetSlotConfig(slotIndex);
        if (cfg == null) return;

        if (!_inventory.TryRemoveCurrency(CurrencyType.Crystals, cfg.priceSkipCrystals))
            return;

        _hatchingService.SkipAll(slotIndex);
        _audio?.PlaySfx(SoundId.BuySell);
    }

    private void TryBuyEgg(EggConfig egg, CurrencyType currency)
    {
        var price = currency == CurrencyType.Coins ? egg.priceCoins : egg.priceCrystals;
        if (!_inventory.TryRemoveCurrency(currency, price))
            return;

        _inventory.AddEgg(egg.id);
        _audio?.PlaySfx(SoundId.BuySell);
    }

    private void RefreshSlots()
    {
        if (_hatchingService == null) return;

        for (int i = 0; i < hatchSlots.Length && i < EggHatchingService.SlotCount; i++)
        {
            var view = hatchSlots[i];
            if (view == null) continue;

            if (_hatchingService.IsSlotEmpty(i))
            {
                view.SetEmpty();
                continue;
            }

            var cfg = _hatchingService.GetSlotConfig(i);
            if (_hatchingService.IsHatched(i))
            {
                view.SetHatched(cfg?.icon);
            }
            else
            {
                var remaining = _hatchingService.GetRemainingSeconds(i);
                bool canSkip = cfg != null &&
                               _inventory.GetCurrencyCount(CurrencyType.Crystals) >= cfg.priceSkipCrystals;
                view.SetHatching(cfg?.icon, FormatTime(remaining), canSkip);
                if (cfg != null)
                {
                    view.SetWatchAdSkipMinutes(cfg.adTimeSkipSeconds);
                    view.SetHatchNowPrice(cfg.priceSkipCrystals);
                }
            }
        }
    }

    private static string FormatTime(float seconds)
    {
        var ts = System.TimeSpan.FromSeconds(seconds);
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}:{ts.Minutes:00}:{ts.Seconds:00}";
        return $"{ts.Minutes:00}:{ts.Seconds:00}";
    }

    private void TryHatchEgg(EggConfig egg)
    {
        _hatchingService?.TryStartHatchingEgg(egg.id);
    }

    private void RefreshEggViews()
    {
        if (_inventory == null || _hatchingService == null) return;

        bool slotAvailable = _hatchingService.HasFreeSlot();
        foreach (var (view, cfg) in _eggViews)
        {
            int count = _inventory.Eggs.Count(id => id == cfg.id);
            view.SetHatchState(count, slotAvailable);
        }
    }

    private void OnCurrencyChanged()
    {
        Refresh();
        RefreshSlots();
    }

    private void Refresh()
    {
        if (_inventory == null)
            return;

        foreach (var (view, cfg) in _eggViews)
        {
            var canCoins = _inventory.GetCurrencyCount(CurrencyType.Coins) >= cfg.priceCoins;
            var canCrystals = _inventory.GetCurrencyCount(CurrencyType.Crystals) >= cfg.priceCrystals;
            view.SetInteractable(canCoins, canCrystals);
        }
    }
}


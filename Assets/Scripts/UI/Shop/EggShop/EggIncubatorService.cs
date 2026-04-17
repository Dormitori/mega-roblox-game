using System;
using System.Collections.Generic;
using System.Linq;
using GamePush;
using UnityEngine;
using VContainer;

public class EggIncubatorService
{
    public const int SlotCount = 3;

    public event Action Changed;

    private readonly ISaveService _saveService;
    private readonly Inventory _inventory;
    private readonly ConfigManager<EggConfig> _eggConfigs;
    private readonly ConfigManager<PetConfig> _petConfigs;
    private readonly PetProgressService _petProgress;

    private EggIncubatorSaveData _data;

    [Inject]
    public EggIncubatorService(
        ISaveService saveService,
        Inventory inventory,
        ConfigManager<EggConfig> eggConfigs,
        ConfigManager<PetConfig> petConfigs,
        PetProgressService petProgress)
    {
        _saveService = saveService;
        _inventory = inventory;
        _eggConfigs = eggConfigs;
        _petConfigs = petConfigs;
        _petProgress = petProgress;

        LoadOrInit();
        SaveTrigger.OnSave += Save;
    }

    private void LoadOrInit()
    {
        if (_saveService.HasKey(SaveKeys.EggIncubator))
        {
            _data = _saveService.Load<EggIncubatorSaveData>(SaveKeys.EggIncubator) ?? new EggIncubatorSaveData();
            _data.slots ??= new EggSlotSaveData[SlotCount];
            if (_data.slots.Length != SlotCount)
                Array.Resize(ref _data.slots, SlotCount);
            return;
        }

        _data = new EggIncubatorSaveData { slots = new EggSlotSaveData[SlotCount] };
        Save();
    }

    public EggSlotSaveData GetSlot(int index)
    {
        if (index < 0 || index >= SlotCount) return null;
        return _data.slots[index];
    }

    public IReadOnlyList<EggSlotSaveData> GetSlots() => _data.slots;

    public bool HasFreeSlot()
    {
        for (var i = 0; i < SlotCount; i++)
            if (_data.slots[i] == null || string.IsNullOrEmpty(_data.slots[i].eggId))
                return true;
        return false;
    }

    public bool TryBuyAndPlace(EggConfig egg, CurrencyType currency)
    {
        if (egg == null) return false;

        var price = currency == CurrencyType.Coins ? egg.priceCoins : egg.priceCrystals;
        if (price <= 0) return false;
        if (!_inventory.TryRemoveCurrency(currency, price)) return false;

        var placed = TryPlaceEgg(egg);
        if (!placed)
        {
            // вернуть валюту, если не удалось поставить
            _inventory.AddCurrency(currency, price);
            return false;
        }

        Changed?.Invoke();
        return true;
    }

    public bool TryPlaceEgg(EggConfig egg)
    {
        if (egg == null) return false;

        var freeIndex = FindFreeSlotIndex();
        if (freeIndex >= 0)
        {
            PutEggToSlot(freeIndex, egg);
            return true;
        }

        var weakest = FindWeakestSlotIndex();
        if (weakest < 0) return false;

        var weakestEgg = ResolveEgg(_data.slots[weakest]?.eggId);
        if (weakestEgg == null) return false;

        if (!IsEggStronger(egg, weakestEgg))
            return false;

        PutEggToSlot(weakest, egg);
        return true;
    }

    public TimeSpan GetRemaining(int slotIndex, DateTime utcNow)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return TimeSpan.Zero;
        var finish = new DateTime(s.finishUtcTicks, DateTimeKind.Utc);
        var remain = finish - utcNow;
        return remain > TimeSpan.Zero ? remain : TimeSpan.Zero;
    }

    public bool CanClaim(int slotIndex, DateTime utcNow)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return false;
        if (s.claimed) return false;
        return GetRemaining(slotIndex, utcNow) <= TimeSpan.Zero;
    }

    public bool TryClaim(int slotIndex)
    {
        var now = DateTime.UtcNow;
        if (!CanClaim(slotIndex, now)) return false;

        var s = GetSlot(slotIndex);
        var egg = ResolveEgg(s.eggId);
        if (egg == null) return false;

        var pet = RollPetFromEgg(egg);
        if (pet == null) return false;

        _petProgress.AddPet(pet.id, 1);

        // очистить слот
        _data.slots[slotIndex] = null;
        Changed?.Invoke();
        return true;
    }

    public bool TrySpeedUpWithCrystals(int slotIndex)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return false;

        var egg = ResolveEgg(s.eggId);
        if (egg == null) return false;

        var cost = GetSpeedUpCrystalCost(egg);
        if (cost <= 0) return false;
        if (!_inventory.TryRemoveCurrency(CurrencyType.Crystals, cost)) return false;

        ApplyTimeReduction(slotIndex, TimeSpan.FromDays(9999)); // моментально
        Changed?.Invoke();
        return true;
    }

    public void TrySpeedUpWithAd(int slotIndex)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return;

        if (!GP_Ads.IsRewardedAvailable())
        {
            // В эмуляторе availability может быть false — всё равно позволим, GP_Ads в редакторе отдаёт reward сразу.
        }

        GP_Ads.ShowRewarded(
            idOrTag: "HATCH_SKIP_5MIN",
            onRewardedReward: _ =>
            {
                ApplyTimeReduction(slotIndex, TimeSpan.FromMinutes(5));
                Changed?.Invoke();
            }
        );
    }

    private void ApplyTimeReduction(int slotIndex, TimeSpan delta)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return;

        var nowTicks = DateTime.UtcNow.Ticks;
        var finish = s.finishUtcTicks;
        var reduced = finish - delta.Ticks;
        s.finishUtcTicks = Math.Max(nowTicks, reduced);
        _data.slots[slotIndex] = s;
    }

    private static int GetSpeedUpCrystalCost(EggConfig egg)
    {
        // По требованиям: simple=5, rare=20, epic=50. Отличаем по hatchDuration.
        if (egg == null) return 0;
        if (egg.hatchDurationSeconds <= 5 * 60) return 5;
        if (egg.hatchDurationSeconds <= 30 * 60) return 20;
        return 50;
    }

    private int FindFreeSlotIndex()
    {
        for (var i = 0; i < SlotCount; i++)
            if (_data.slots[i] == null || string.IsNullOrEmpty(_data.slots[i].eggId))
                return i;
        return -1;
    }

    private int FindWeakestSlotIndex()
    {
        var weakestIndex = -1;
        EggConfig weakestEgg = null;

        for (var i = 0; i < SlotCount; i++)
        {
            var id = _data.slots[i]?.eggId;
            if (string.IsNullOrEmpty(id)) continue;
            var egg = ResolveEgg(id);
            if (egg == null) continue;

            if (weakestEgg == null || IsEggStronger(weakestEgg, egg))
            {
                weakestEgg = egg;
                weakestIndex = i;
            }
        }

        return weakestIndex;
    }

    private static bool IsEggStronger(EggConfig a, EggConfig b)
    {
        // Базовое сравнение по времени (Simple 5m < Rare 30m < Epic 2h).
        return a.hatchDurationSeconds > b.hatchDurationSeconds;
    }

    private void PutEggToSlot(int slotIndex, EggConfig egg)
    {
        var now = DateTime.UtcNow;
        var finish = now.AddSeconds(Mathf.Max(1, egg.hatchDurationSeconds));
        _data.slots[slotIndex] = new EggSlotSaveData
        {
            eggId = egg.id,
            startUtcTicks = now.Ticks,
            finishUtcTicks = finish.Ticks,
            claimed = false
        };
    }

    private EggConfig ResolveEgg(string eggId)
    {
        if (string.IsNullOrEmpty(eggId)) return null;
        return _eggConfigs.Configs.FirstOrDefault(e => e != null && e.id == eggId);
    }

    private PetConfig RollPetFromEgg(EggConfig egg)
    {
        if (egg == null) return null;

        var rarity = RollRarity(egg.rarityChances);
        var candidates = _petConfigs.Configs.Where(p => p != null && p.rarity == rarity && !string.IsNullOrEmpty(p.id)).ToList();
        if (candidates.Count == 0)
            return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private static PetRarity RollRarity(List<EggConfig.RarityChance> chances)
    {
        if (chances == null || chances.Count == 0)
            return PetRarity.Common;

        var total = 0f;
        foreach (var c in chances)
            total += Mathf.Max(0f, c.percent);

        if (total <= 0f)
            return chances[0].rarity;

        var r = UnityEngine.Random.Range(0f, total);
        var acc = 0f;
        foreach (var c in chances)
        {
            acc += Mathf.Max(0f, c.percent);
            if (r <= acc)
                return c.rarity;
        }

        return chances[^1].rarity;
    }

    private void Save()
    {
        _saveService.Save(SaveKeys.EggIncubator, _data);
    }
}

[Serializable]
public class EggIncubatorSaveData
{
    public EggSlotSaveData[] slots;
}

[Serializable]
public class EggSlotSaveData
{
    public string eggId;
    public long startUtcTicks;
    public long finishUtcTicks;
    public bool claimed;
}


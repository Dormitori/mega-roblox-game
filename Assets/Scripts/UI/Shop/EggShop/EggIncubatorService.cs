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
    private readonly PetEquipService _petEquip;

    private EggIncubatorSaveData _data;

    [Inject]
    public EggIncubatorService(
        ISaveService saveService,
        Inventory inventory,
        ConfigManager<EggConfig> eggConfigs,
        ConfigManager<PetConfig> petConfigs,
        PetProgressService petProgress,
        PetEquipService petEquip)
    {
        _saveService = saveService;
        _inventory = inventory;
        _eggConfigs = eggConfigs;
        _petConfigs = petConfigs;
        _petProgress = petProgress;
        _petEquip = petEquip;

        LoadOrInit();
        SaveTrigger.OnSave += Save;
    }

    private void LoadOrInit()
    {
        if (_saveService.HasKey(SaveKeys.EggIncubator))
        {
            _data = _saveService.Load<EggIncubatorSaveData>(SaveKeys.EggIncubator) ?? new EggIncubatorSaveData();
            _data.slots ??= new EggSlotSaveData[SlotCount];
            _data.eggInventory ??= new Dictionary<string, int>();
            if (_data.slots.Length != SlotCount)
                Array.Resize(ref _data.slots, SlotCount);
            return;
        }

        _data = new EggIncubatorSaveData
        {
            slots = new EggSlotSaveData[SlotCount],
            eggInventory = new Dictionary<string, int>()
        };
        Save();
    }

    public EggSlotSaveData GetSlot(int index)
    {
        if (index < 0 || index >= SlotCount) return null;
        return _data.slots[index];
    }

    public IReadOnlyList<EggSlotSaveData> GetSlots() => _data.slots;

    public int GetEggInventoryCount(string eggId)
    {
        if (string.IsNullOrEmpty(eggId)) return 0;
        return _data.eggInventory.TryGetValue(eggId, out var c) ? c : 0;
    }

    public bool HasAnyEggsInInventory() => _data.eggInventory.Values.Any(v => v > 0);

    /// <summary>Покупка яйца в магазине — только в инвентарь, без установки в слот.</summary>
    public bool TryPurchaseEgg(EggConfig egg, CurrencyType currency)
    {
        if (egg == null) return false;

        var price = currency == CurrencyType.Coins ? egg.priceCoins : egg.priceCrystals;
        if (price <= 0) return false;
        if (!_inventory.TryRemoveCurrency(currency, price)) return false;

        _data.eggInventory.TryGetValue(egg.id, out var n);
        _data.eggInventory[egg.id] = n + 1;
        Changed?.Invoke();
        return true;
    }

    /// <summary>Пустой слот: кладёт самое «крутое» яйцо из инвентаря (дольше инкубация = выше тир).</summary>
    public bool TryPlaceBestEggInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return false;

        var slot = GetSlot(slotIndex);
        if (slot != null && !string.IsNullOrEmpty(slot.eggId))
            return false;

        var best = GetBestEggConfigFromInventory();
        if (best == null) return false;

        if (!TryConsumeEggFromInventory(best.id, 1))
            return false;

        PutEggToSlot(slotIndex, best);
        Changed?.Invoke();
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

    public bool IsSlotEmpty(int slotIndex)
    {
        var s = GetSlot(slotIndex);
        return s == null || string.IsNullOrEmpty(s.eggId);
    }

    /// <summary>Таймер закончился, ролл выполнен — можно открывать UI вылупления.</summary>
    public bool IsReadyForHatchUi(int slotIndex, DateTime utcNow)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return false;
        if (GetRemaining(slotIndex, utcNow) > TimeSpan.Zero) return false;
        EnsureRolledPetForSlot(slotIndex);
        return !string.IsNullOrEmpty(s.rolledPetId);
    }

    public string GetRolledPetId(int slotIndex)
    {
        EnsureRolledPetForSlot(slotIndex);
        return GetSlot(slotIndex)?.rolledPetId;
    }

    /// <summary>Когда время вышло — один раз роллим пета (сохраняем до получения награды).</summary>
    public void EnsureRolledPetForSlot(int slotIndex)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return;
        if (GetRemaining(slotIndex, DateTime.UtcNow) > TimeSpan.Zero) return;
        if (!string.IsNullOrEmpty(s.rolledPetId)) return;

        var egg = ResolveEgg(s.eggId);
        var pet = RollPetFromEgg(egg);
        if (pet == null) return;

        s.rolledPetId = pet.id;
        _data.slots[slotIndex] = s;
        Changed?.Invoke();
    }

    /// <summary>После UI вылупления: выдать пета в коллекцию; при equip — сделать текущим и обновить визуал.</summary>
    public bool TryFinalizeHatch(int slotIndex, bool equipAsCurrent)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.rolledPetId)) return false;

        var petId = s.rolledPetId;
        _petProgress.AddPet(petId, 1);
        if (equipAsCurrent)
            _petEquip.TryEquip(petId);

        _data.slots[slotIndex] = null;
        Changed?.Invoke();
        return true;
    }

    public bool TrySpeedUpWithCrystals(int slotIndex)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return false;
        if (!string.IsNullOrEmpty(s.rolledPetId)) return false;

        var egg = ResolveEgg(s.eggId);
        if (egg == null) return false;

        var cost = GetSpeedUpCrystalCost(egg);
        if (cost <= 0) return false;
        if (!_inventory.TryRemoveCurrency(CurrencyType.Crystals, cost)) return false;

        ApplyTimeReduction(slotIndex, TimeSpan.FromDays(9999));
        EnsureRolledPetForSlot(slotIndex);
        Changed?.Invoke();
        return true;
    }

    public void TrySpeedUpWithAd(int slotIndex)
    {
        var s = GetSlot(slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return;
        if (!string.IsNullOrEmpty(s.rolledPetId)) return;

        var egg = ResolveEgg(s.eggId);
        var skip = egg != null && egg.adTimeSkipSeconds > 0
            ? TimeSpan.FromSeconds(egg.adTimeSkipSeconds)
            : TimeSpan.FromMinutes(3);

        GP_Ads.ShowRewarded(
            idOrTag: "HATCH_SKIP_TIME",
            onRewardedReward: _ =>
            {
                ApplyTimeReduction(slotIndex, skip);
                EnsureRolledPetForSlot(slotIndex);
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
        if (egg == null) return 0;
        if (egg.hatchDurationSeconds <= 5 * 60) return 5;
        if (egg.hatchDurationSeconds <= 30 * 60) return 20;
        return 50;
    }

    private EggConfig GetBestEggConfigFromInventory()
    {
        EggConfig best = null;
        foreach (var kv in _data.eggInventory)
        {
            if (kv.Value <= 0) continue;
            var cfg = ResolveEgg(kv.Key);
            if (cfg == null) continue;
            if (best == null || IsEggStronger(cfg, best))
                best = cfg;
        }
        return best;
    }

    private bool TryConsumeEggFromInventory(string eggId, int amount)
    {
        if (!_data.eggInventory.TryGetValue(eggId, out var c) || c < amount)
            return false;
        c -= amount;
        if (c <= 0)
            _data.eggInventory.Remove(eggId);
        else
            _data.eggInventory[eggId] = c;
        return true;
    }

    private static bool IsEggStronger(EggConfig a, EggConfig b)
    {
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
            rolledPetId = null
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
    public Dictionary<string, int> eggInventory;
    public EggSlotSaveData[] slots;
}

[Serializable]
public class EggSlotSaveData
{
    public string eggId;
    public long startUtcTicks;
    public long finishUtcTicks;
    /// <summary>Заполняется когда инкубация закончилась; награда выдаётся только через TryFinalizeHatch.</summary>
    public string rolledPetId;
}

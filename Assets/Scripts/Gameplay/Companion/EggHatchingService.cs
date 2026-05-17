using System;
using System.Collections.Generic;
using System.Linq;
using GamePush;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EggHatchingService : ITickable
{
    public const int SlotCount = 3;

    public event Action SlotsChanged;
    public event Action<string> PetHatched;

    private readonly ISaveService _saveService;
    private readonly Inventory _inventory;
    private readonly PetProgressService _petProgress;
    private readonly ConfigManager<EggConfig> _eggConfigs;
    private readonly ConfigManager<PetConfig> _petConfigs;

    private EggIncubatorSaveData _data;
    private float _tickAccum;
    private const float TickInterval = 1f;

    [Inject]
    public EggHatchingService(
        ISaveService saveService,
        Inventory inventory,
        PetProgressService petProgress,
        ConfigManager<EggConfig> eggConfigs,
        ConfigManager<PetConfig> petConfigs)
    {
        _saveService = saveService;
        _inventory = inventory;
        _petProgress = petProgress;
        _eggConfigs = eggConfigs;
        _petConfigs = petConfigs;

        LoadOrInit();
        SaveTrigger.OnSave += Save;
    }

    public void Tick()
    {
        _tickAccum += Time.deltaTime;
        if (_tickAccum < TickInterval) return;
        _tickAccum -= TickInterval;
        SlotsChanged?.Invoke();
    }

    private void LoadOrInit()
    {
        if (_saveService.HasKey(SaveKeys.EggIncubator))
        {
            _data = _saveService.Load<EggIncubatorSaveData>(SaveKeys.EggIncubator);
            _data ??= new EggIncubatorSaveData();
            _data.Slots ??= new List<EggHatchSlotData>();
        }
        else
        {
            _data = new EggIncubatorSaveData();
        }

        while (_data.Slots.Count < SlotCount)
            _data.Slots.Add(new EggHatchSlotData());

        for (int i = 0; i < _data.Slots.Count; i++)
            _data.Slots[i] ??= new EggHatchSlotData();
    }

    public EggHatchSlotData GetSlot(int index) => _data.Slots[index];

    public bool IsSlotEmpty(int index) => string.IsNullOrEmpty(_data.Slots[index].EggId);

    public bool IsHatched(int index)
    {
        var slot = _data.Slots[index];
        return !string.IsNullOrEmpty(slot.EggId) && DateTime.UtcNow.Ticks >= slot.EndTimeUtcTicks;
    }

    public float GetRemainingSeconds(int index)
    {
        var slot = _data.Slots[index];
        if (string.IsNullOrEmpty(slot.EggId)) return 0f;
        var ticks = slot.EndTimeUtcTicks - DateTime.UtcNow.Ticks;
        return Mathf.Max(0f, ticks / (float)TimeSpan.TicksPerSecond);
    }

    public EggConfig GetSlotConfig(int index)
    {
        var eggId = _data.Slots[index].EggId;
        if (string.IsNullOrEmpty(eggId)) return null;
        return _eggConfigs.Configs.FirstOrDefault(e => e != null && e.id == eggId);
    }

    public bool HasFreeSlot()
    {
        for (int i = 0; i < SlotCount; i++)
            if (IsSlotEmpty(i)) return true;
        return false;
    }

    public bool TryStartHatchingEgg(string eggId)
    {
        int freeSlot = -1;
        for (int i = 0; i < SlotCount; i++)
        {
            if (IsSlotEmpty(i)) { freeSlot = i; break; }
        }
        if (freeSlot < 0) return false;
        if (!_inventory.RemoveEgg(eggId)) return false;

        var cfg = _eggConfigs.Configs.FirstOrDefault(e => e != null && e.id == eggId);
        int duration = cfg?.hatchDurationSeconds ?? 60;

        _data.Slots[freeSlot] = new EggHatchSlotData
        {
            EggId = eggId,
            EndTimeUtcTicks = DateTime.UtcNow.AddSeconds(duration).Ticks
        };

        Save();
        SlotsChanged?.Invoke();
        return true;
    }

    public void SkipByAd(int slotIndex)
    {
        if (IsSlotEmpty(slotIndex)) return;
        var cfg = GetSlotConfig(slotIndex);
        int skip = cfg?.adTimeSkipSeconds ?? 180;
        
        GP_Ads.ShowRewarded(
            idOrTag: "HATCH_SKIP_TIME",
            onRewardedReward: _ =>
            {
                var slot = _data.Slots[slotIndex];
                slot.EndTimeUtcTicks -= TimeSpan.FromSeconds(skip).Ticks;
                if (slot.EndTimeUtcTicks < DateTime.UtcNow.Ticks)
                    slot.EndTimeUtcTicks = DateTime.UtcNow.Ticks;

                Save();
                SlotsChanged?.Invoke();
            }
        );
    }

    public void SkipAll(int slotIndex)
    {
        if (IsSlotEmpty(slotIndex)) return;
        var slot = _data.Slots[slotIndex];
        slot.EndTimeUtcTicks = DateTime.UtcNow.Ticks;

        Save();
        SlotsChanged?.Invoke();
    }

    public void TryCollect(int slotIndex)
    {
        if (!IsHatched(slotIndex)) return;
        Collect(slotIndex);
    }

    private void Collect(int slotIndex)
    {
        var cfg = GetSlotConfig(slotIndex);
        if (cfg != null)
        {
            var petId = RollPet(cfg);
            if (!string.IsNullOrEmpty(petId))
            {
                _petProgress.AddPet(petId);
                PetHatched?.Invoke(petId);
            }
        }

        _data.Slots[slotIndex] = new EggHatchSlotData();
        Save();
        SlotsChanged?.Invoke();
    }

    private string RollPet(EggConfig cfg)
    {
        if (cfg.rarityChances == null || cfg.rarityChances.Count == 0) return null;

        float roll = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0f;
        PetRarity chosenRarity = cfg.rarityChances[0].rarity;

        foreach (var rc in cfg.rarityChances)
        {
            cumulative += rc.percent;
            if (roll < cumulative)
            {
                chosenRarity = rc.rarity;
                break;
            }
        }

        var candidates = _petConfigs.Configs
            .Where(p => p != null && p.rarity == chosenRarity)
            .ToList();

        if (candidates.Count == 0) return null;
        return candidates[UnityEngine.Random.Range(0, candidates.Count)].id;
    }

    private void Save() => _saveService.Save(SaveKeys.EggIncubator, _data);
}

[Serializable]
public class EggIncubatorSaveData
{
    public List<EggHatchSlotData> Slots = new();
}

[Serializable]
public class EggHatchSlotData
{
    public string EggId;
    public long EndTimeUtcTicks;
}

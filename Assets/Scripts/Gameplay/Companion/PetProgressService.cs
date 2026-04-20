using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class PetProgressService
{
    public event Action Changed;

    private readonly ISaveService _saveService;
    private readonly ConfigManager<PetConfig> _petConfigs;

    private PetProgressSaveData _data;

    [Inject]
    public PetProgressService(ISaveService saveService, ConfigManager<PetConfig> petConfigs)
    {
        _saveService = saveService;
        _petConfigs = petConfigs;

        LoadOrInit();
        SaveTrigger.OnSave += Save;
    }

    private void LoadOrInit()
    {
        if (_saveService.HasKey(SaveKeys.PetProgress))
        {
            _data = _saveService.Load<PetProgressSaveData>(SaveKeys.PetProgress) ?? new PetProgressSaveData();
            _data.ownedPetCounts ??= new Dictionary<string, int>();
            return;
        }

        _data = new PetProgressSaveData
        {
            ownedPetCounts = new Dictionary<string, int>(),
            equippedPetId = null
        };
        Save();
    }

    public int GetOwnedCount(string petId)
    {
        if (string.IsNullOrEmpty(petId)) return 0;
        return _data.ownedPetCounts.TryGetValue(petId, out var c) ? c : 0;
    }

    public IReadOnlyDictionary<string, int> GetAllOwnedCounts() => _data.ownedPetCounts;

    public string EquippedPetId => _data.equippedPetId;

    public PetConfig GetEquippedPetConfig()
    {
        if (string.IsNullOrEmpty(_data.equippedPetId))
            return null;
        return _petConfigs.Configs.FirstOrDefault(p => p != null && p.id == _data.equippedPetId);
    }

    public void AddPet(string petId, int amount = 1)
    {
        if (string.IsNullOrEmpty(petId) || amount <= 0) return;

        _data.ownedPetCounts.TryGetValue(petId, out var current);
        _data.ownedPetCounts[petId] = Mathf.Max(0, current + amount);
        Changed?.Invoke();
        Persist();
    }

    public bool CanEquip(string petId) => GetOwnedCount(petId) > 0;

    public bool TryEquip(string petId)
    {
        if (!CanEquip(petId))
            return false;

        _data.equippedPetId = petId;
        Changed?.Invoke();
        Persist();
        return true;
    }

    private void Save()
    {
        Persist();
    }

    private void Persist()
    {
        _saveService.Save(SaveKeys.PetProgress, _data);
    }
}

[Serializable]
public class PetProgressSaveData
{
    public Dictionary<string, int> ownedPetCounts;
    public string equippedPetId;
}


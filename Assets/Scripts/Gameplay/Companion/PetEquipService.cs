using System.Linq;
using UnityEngine;
using VContainer;

public class PetEquipService
{
    private readonly PetProgressService _progress;
    private readonly ConfigManager<PetConfig> _petConfigs;

    [Inject]
    public PetEquipService(PetProgressService progress, ConfigManager<PetConfig> petConfigs)
    {
        _progress = progress;
        _petConfigs = petConfigs;
    }

    public bool TryEquip(string petId)
    {
        if (!_progress.TryEquip(petId))
            return false;

        ApplyEquippedToWorld();
        return true;
    }

    public void ApplyEquippedToWorld()
    {
        var equippedId = _progress.EquippedPetId;
        if (string.IsNullOrEmpty(equippedId))
            return;

        var cfg = _petConfigs.Configs.FirstOrDefault(p => p != null && p.id == equippedId);
        if (cfg == null)
            return;

        var visual = Object.FindFirstObjectByType<CompanionPetVisual>();
        if (visual == null)
            return;

        visual.Apply(cfg);
    }
}


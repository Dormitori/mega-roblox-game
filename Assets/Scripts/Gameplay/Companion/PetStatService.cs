using VContainer;

public class PetStatService
{
    private readonly PetProgressService _progress;

    [Inject]
    public PetStatService(PetProgressService progress)
    {
        _progress = progress;
    }

    // Returns 1 + bonusPercent/100 for the active stat type, or 1 if no match
    public float GetMultiplier(PetStatType statType)
    {
        var cfg = _progress.GetEquippedPetConfig();
        if (cfg == null || cfg.statType != statType)
            return 1f;
        return 1f + cfg.bonusPercent / 100f;
    }

    // Returns additive ore-chance bonus in [0,1] range, or 0 if no match
    public float GetRareOreChanceBonus()
    {
        var cfg = _progress.GetEquippedPetConfig();
        if (cfg == null || cfg.statType != PetStatType.RareOreChancePercent)
            return 0f;
        return cfg.bonusPercent / 100f;
    }
}

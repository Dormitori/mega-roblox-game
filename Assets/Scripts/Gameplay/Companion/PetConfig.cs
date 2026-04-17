using I2.Loc;
using UnityEngine;

public enum PetRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

public enum PetStatType
{
    GoldPricePercent,
    BlockDamagePercent,
    AttackSpeedPercent,
    RareOreChancePercent
}

[CreateAssetMenu(fileName = "PetConfig", menuName = "Config/Pet/PetConfig")]
public class PetConfig : ScriptableObject
{
    [Tooltip("Уникальный id для сейва/магазина")]
    public string id;

    [Tooltip("Термин I2, например Pets/FireCub")]
    public string nameTerm;

    public Sprite icon;
    public PetRarity rarity;

    [Header("Bonus")]
    public PetStatType statType;
    [Tooltip("Например 5/8/12/16/20/25")]
    public float bonusPercent;

    [Tooltip("Префаб визуала с Animator (на корне или в детях)")]
    public GameObject visualPrefab;

    public string LocalizedName =>
        string.IsNullOrEmpty(nameTerm) ? id : LocalizationManager.GetTranslation(nameTerm);
}

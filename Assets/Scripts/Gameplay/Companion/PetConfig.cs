using I2.Loc;
using UnityEngine;

public enum PetRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
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
    public int price;
    public CurrencyType currency = CurrencyType.Coins;

    [Tooltip("Префаб визуала с Animator (на корне или в детях)")]
    public GameObject visualPrefab;

    public string LocalizedName =>
        string.IsNullOrEmpty(nameTerm) ? id : LocalizationManager.GetTranslation(nameTerm);
}

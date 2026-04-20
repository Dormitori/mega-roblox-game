using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

[CreateAssetMenu(fileName = "EggConfig", menuName = "Config/Pet/EggConfig")]
public class EggConfig : ScriptableObject
{
    public string id;

    public string nameTerm;

    public Sprite icon;

    [Header("Shop prices")]
    public int priceCoins;
    public int priceCrystals;

    [Header("Hatching")]
    public int hatchDurationSeconds;

    [Tooltip("Сколько секунд снимает rewarded-реклама с таймера (по ТЗ ~3 минуты).")]
    public int adTimeSkipSeconds = 180;

    public GameObject eggWorldPrefab;

    [Header("Drop chances by rarity (percent, sum = 100)")]
    public List<RarityChance> rarityChances = new();

    public string LocalizedName =>
        string.IsNullOrEmpty(nameTerm) ? id : LocalizationManager.GetTranslation(nameTerm);

    [Serializable]
    public struct RarityChance
    {
        public PetRarity rarity;
        public float percent;
    }
}


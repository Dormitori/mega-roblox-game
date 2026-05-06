using System;
using System.Collections.Generic;
using System.IO;
using I2.Loc;
using UnityEditor;
using UnityEngine;

public static class PetEggContentGenerator
{
    private const string PetResourcesDir = "Assets/Resources/PetConfig";
    private const string EggResourcesDir = "Assets/Resources/EggConfig";

    [InitializeOnLoadMethod]
    private static void EnsureI2TermsOnLoad()
    {
        // Не создаём ассеты автоматически (чтобы не шуметь), но термины держим актуальными.
        EnsureI2Terms();
    }

    [MenuItem("Tools/Pets/Ensure I2 terms")]
    public static void EnsureI2TermsMenu() => EnsureI2Terms();

    [MenuItem("Tools/Pets/Generate Pet + Egg Configs (no icons/prefabs)")]
    public static void GenerateConfigsMenu()
    {
        EnsureFolders();
        GeneratePetConfigs();
        GenerateEggConfigs();
        EnsureI2Terms();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated PetConfig + EggConfig assets (icons/prefabs are empty).");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(PetResourcesDir))
            AssetDatabase.CreateFolder("Assets/Resources", "PetConfig");

        if (!AssetDatabase.IsValidFolder(EggResourcesDir))
            AssetDatabase.CreateFolder("Assets/Resources", "EggConfig");
    }

    private static void GeneratePetConfigs()
    {
        foreach (var p in PetDefs())
        {
            var path = $"{PetResourcesDir}/{p.id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<PetConfig>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PetConfig>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.id = p.id;
            asset.nameTerm = $"Pets/{p.id}";
            asset.rarity = p.rarity;
            asset.statType = p.stat;
            asset.bonusPercent = p.bonus;

            EditorUtility.SetDirty(asset);
        }
    }

    private static void GenerateEggConfigs()
    {
        foreach (var e in EggDefs())
        {
            var path = $"{EggResourcesDir}/{e.id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<EggConfig>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EggConfig>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.id = e.id;
            asset.nameTerm = $"Eggs/{e.id}";
            asset.priceCoins = e.priceCoins;
            asset.priceCrystals = e.priceCrystals;
            asset.hatchDurationSeconds = e.hatchSeconds;
            asset.adTimeSkipSeconds = 180;
            asset.rarityChances = new List<EggConfig.RarityChance>(e.chances);

            EditorUtility.SetDirty(asset);
        }
    }

    private static void EnsureI2Terms()
    {
        var sourceAsset = Resources.Load<LanguageSourceAsset>("I2Languages");
        if (sourceAsset == null || sourceAsset.mSource == null)
            return;

        var src = sourceAsset.mSource;
        src.UpdateDictionary(force: true);

        var en = src.GetLanguageIndex("English", AllowDiscartingRegion: true, SkipDisabled: false);
        var ru = src.GetLanguageIndex("Russian", AllowDiscartingRegion: true, SkipDisabled: false);

        EnsureTerm(src, "EGG_SHOP", "Egg shop", "Магазин яиц", en, ru);
        EnsureTerm(src, "TAKE", "Take", "Взять", en, ru);
        EnsureTerm(src, "CLAIM", "Claim", "Забрать", en, ru);
        EnsureTerm(src, "HATCH", "Hatch", "Вылупление", en, ru);
        EnsureTerm(src, "SPEED_UP", "Speed up", "Ускорить", en, ru);

        EnsureTerm(src, "Tutorial/CollectSand", "Collect sand {0}/10", "Собери песок {0}/10", en, ru);
        EnsureTerm(src, "Tutorial/CollectCoal", "Collect coal {0}/2", "Собери уголь {0}/2", en, ru);

        EnsureTerm(src, "Eggs/Simple", "Simple egg", "Простое яйцо", en, ru);
        EnsureTerm(src, "Eggs/Rare", "Rare egg", "Редкое яйцо", en, ru);
        EnsureTerm(src, "Eggs/Epic", "Epic egg", "Эпическое яйцо", en, ru);

        EnsureTerm(src, "PetStat/Gold", "Gold", "Золото", en, ru);
        EnsureTerm(src, "PetStat/Damage", "Damage", "Урон", en, ru);
        EnsureTerm(src, "PetStat/AttackSpeed", "Attack speed", "Скорость атаки", en, ru);
        EnsureTerm(src, "PetStat/RareOreChance", "Rare ore chance", "Шанс редкой руды", en, ru);

        foreach (var p in PetDefs())
            EnsureTerm(src, $"Pets/{p.id}", p.enName, p.ruName, en, ru);

        EditorUtility.SetDirty(sourceAsset);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureTerm(LanguageSourceData src, string term, string enVal, string ruVal, int enIdx, int ruIdx)
    {
        var td = src.GetTermData(term);
        if (td == null)
            td = src.AddTerm(term);

        if (enIdx >= 0)
            td.SetTranslation(enIdx, enVal);
        if (ruIdx >= 0)
            td.SetTranslation(ruIdx, ruVal);

        src.UpdateDictionary(force: true);
    }

    private static IEnumerable<(string id, string enName, string ruName, PetRarity rarity, PetStatType stat, float bonus)> PetDefs()
    {
        // Common (5%)
        yield return ("Beaver", "Beaver", "Бобёр", PetRarity.Common, PetStatType.BlockDamagePercent, 5);
        yield return ("Bee", "Bee", "Пчела", PetRarity.Common, PetStatType.AttackSpeedPercent, 5);
        yield return ("Bunny", "Bunny", "Заяц", PetRarity.Common, PetStatType.GoldPricePercent, 5);
        yield return ("Chick", "Chick", "Цыплёнок", PetRarity.Common, PetStatType.RareOreChancePercent, 5);

        // Uncommon (8%)
        yield return ("Dog", "Dog", "Пёс", PetRarity.Uncommon, PetStatType.BlockDamagePercent, 8);
        yield return ("Cat", "Cat", "Кот", PetRarity.Uncommon, PetStatType.AttackSpeedPercent, 8);
        yield return ("Crab", "Crab", "Краб", PetRarity.Uncommon, PetStatType.GoldPricePercent, 8);
        yield return ("Cow", "Cow", "Корова", PetRarity.Uncommon, PetStatType.RareOreChancePercent, 8);

        // Rare (12%)
        yield return ("Parrot", "Parrot", "Попугай", PetRarity.Rare, PetStatType.BlockDamagePercent, 12);
        yield return ("Penguin", "Penguin", "Пингвин", PetRarity.Rare, PetStatType.AttackSpeedPercent, 12);
        yield return ("Pig", "Pig", "Свинья", PetRarity.Rare, PetStatType.GoldPricePercent, 12);
        yield return ("Bear", "Bear", "Медведь", PetRarity.Rare, PetStatType.RareOreChancePercent, 12);

        // Epic (16%)
        yield return ("Caterpillar", "Caterpillar", "Гусеница", PetRarity.Epic, PetStatType.BlockDamagePercent, 16);
        yield return ("Deer", "Deer", "Олень", PetRarity.Epic, PetStatType.AttackSpeedPercent, 16);
        yield return ("Elephant", "Elephant", "Слон", PetRarity.Epic, PetStatType.GoldPricePercent, 16);
        yield return ("Fish", "Fish", "Рыбка", PetRarity.Epic, PetStatType.RareOreChancePercent, 16);

        // Legendary (20%)
        yield return ("Fox", "Fox", "Лиса", PetRarity.Legendary, PetStatType.BlockDamagePercent, 20);
        yield return ("Giraffe", "Giraffe", "Жираф", PetRarity.Legendary, PetStatType.AttackSpeedPercent, 20);
        yield return ("Boar", "Boar", "Кабан", PetRarity.Legendary, PetStatType.GoldPricePercent, 20);
        yield return ("Koala", "Koala", "Коала", PetRarity.Legendary, PetStatType.RareOreChancePercent, 20);

        // Mythic (25%)
        yield return ("Lion", "Lion", "Лев", PetRarity.Mythic, PetStatType.BlockDamagePercent, 25);
        yield return ("Monkey", "Monkey", "Обезьяна", PetRarity.Mythic, PetStatType.AttackSpeedPercent, 25);
        yield return ("Panda", "Panda", "Панда", PetRarity.Mythic, PetStatType.GoldPricePercent, 25);
        yield return ("Tiger", "Tiger", "Тигр", PetRarity.Mythic, PetStatType.RareOreChancePercent, 25);
    }

    private static IEnumerable<(string id, int priceCoins, int priceCrystals, int hatchSeconds, EggConfig.RarityChance[] chances)> EggDefs()
    {
        yield return ("Simple", 1500, 15, 5 * 60, new[]
        {
            new EggConfig.RarityChance{rarity = PetRarity.Common, percent = 65f},
            new EggConfig.RarityChance{rarity = PetRarity.Uncommon, percent = 25f},
            new EggConfig.RarityChance{rarity = PetRarity.Rare, percent = 7f},
            new EggConfig.RarityChance{rarity = PetRarity.Epic, percent = 2.5f},
            new EggConfig.RarityChance{rarity = PetRarity.Legendary, percent = 0.45f},
            new EggConfig.RarityChance{rarity = PetRarity.Mythic, percent = 0.05f},
        });

        yield return ("Rare", 15000, 75, 30 * 60, new[]
        {
            new EggConfig.RarityChance{rarity = PetRarity.Common, percent = 35f},
            new EggConfig.RarityChance{rarity = PetRarity.Uncommon, percent = 30f},
            new EggConfig.RarityChance{rarity = PetRarity.Rare, percent = 20f},
            new EggConfig.RarityChance{rarity = PetRarity.Epic, percent = 10f},
            new EggConfig.RarityChance{rarity = PetRarity.Legendary, percent = 4.5f},
            new EggConfig.RarityChance{rarity = PetRarity.Mythic, percent = 0.5f},
        });

        yield return ("Epic", 70000, 200, 2 * 60 * 60, new[]
        {
            new EggConfig.RarityChance{rarity = PetRarity.Common, percent = 15f},
            new EggConfig.RarityChance{rarity = PetRarity.Uncommon, percent = 20f},
            new EggConfig.RarityChance{rarity = PetRarity.Rare, percent = 25f},
            new EggConfig.RarityChance{rarity = PetRarity.Epic, percent = 20f},
            new EggConfig.RarityChance{rarity = PetRarity.Legendary, percent = 15f},
            new EggConfig.RarityChance{rarity = PetRarity.Mythic, percent = 5f},
        });
    }
}


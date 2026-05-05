using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Gameplay/MineBalanceConfig")]
public class MineBalanceConfig : ScriptableObject
{
    [Header("База на глубине 0 (песок)")]
    public float baseSandHp = 3f;
    public float baseSandSellPrice = 2f;

    [Header("Рост по глубине")]
    [Tooltip("HP = baseSandHp * pow(hpPerLevelMultiplier, depth)")]
    public float hpPerLevelMultiplier = 1.03f;

    [Tooltip("Цена руды = baseSandSellPrice * pow(pricePerLevelMultiplier, depth)")]
    public float pricePerLevelMultiplier = 1.045f;

    [Header("Руда")]
    public float oreHpMultiplierVsDepthBase = 1.8f;
    [Range(0f, 1f)] public float oreChanceBase = 0.1f;
    public float oreChancePerDepth = 0.0001f;
    [Tooltip("0 = без верхней границы")]
    public float oreChanceMax = 0.25f;

    [Header("Спавн породы (треугольные веса)")]
    [Tooltip("Чем больше — тем шире «холм» вокруг центра глубины")]
    public float spawnCenterFalloffWidth = 100f;

    public TerrainSpawnEntry[] terrainEntries = Array.Empty<TerrainSpawnEntry>();
    public OreSpawnEntry[] oreEntries = Array.Empty<OreSpawnEntry>();

    [Header("Визуал при отсутствии префаба")]
    public BlockType fallbackVisualBlockType = BlockType.Stone;

    public float GetOreChance(int depth)
    {
        var p = oreChanceBase + depth * oreChancePerDepth;
        if (oreChanceMax > 0f)
            p = Mathf.Min(p, oreChanceMax);
        return Mathf.Clamp01(p);
    }

    public float GetBaseHp(int depth)
    {
        return baseSandHp * Mathf.Pow(hpPerLevelMultiplier, depth);
    }

    public float GetOreBaseSellPrice(int depth)
    {
        return baseSandSellPrice * Mathf.Pow(pricePerLevelMultiplier, depth);
    }

    public TerrainSpawnEntry GetTerrainEntry(BlockType type)
    {
        foreach (var e in terrainEntries)
            if (e.blockType == type)
                return e;
        return null;
    }

    public OreSpawnEntry GetOreEntry(BlockType type)
    {
        foreach (var e in oreEntries)
            if (e.blockType == type)
                return e;
        return null;
    }

    public BlockType PickTerrain(int depth, System.Func<float> rng)
    {
        if (terrainEntries == null || terrainEntries.Length == 0)
            return BlockType.Sand;

        var eligible = terrainEntries
            .Where(e => e != null && depth >= e.minDepth && (e.maxDepth <= 0 || depth <= e.maxDepth))
            .ToArray();

        if (eligible.Length == 0)
            return BlockType.Sand;

        var weights = new float[eligible.Length];
        var w = Mathf.Max(1f, spawnCenterFalloffWidth);
        float sum = 0f;
        for (var i = 0; i < eligible.Length; i++)
        {
            var d = Mathf.Abs(depth - eligible[i].depthCenter);
            weights[i] = Mathf.Max(0f, 1f - d / w);
            sum += weights[i];
        }

        if (sum < 1e-6f)
        {
            var ri = Mathf.Clamp(Mathf.FloorToInt(rng() * eligible.Length), 0, eligible.Length - 1);
            return eligible[ri].blockType;
        }

        var r = rng() * sum;
        var acc = 0f;
        for (var i = 0; i < weights.Length; i++)
        {
            acc += weights[i];
            if (r <= acc)
                return eligible[i].blockType;
        }

        return eligible[^1].blockType;
    }

    public bool TryPickOre(int depth, Func<float> rng, Predicate<BlockType> typeAllowed, out BlockType oreType)
    {
        oreType = default;
        if (oreEntries == null || oreEntries.Length == 0)
            return false;

        var eligible = oreEntries
            .Where(e => depth >= e.minDepth && typeAllowed(e.blockType))
            .ToArray();

        if (eligible.Length == 0)
            return false;

        var idx = Mathf.Clamp(Mathf.FloorToInt(rng() * eligible.Length), 0, eligible.Length - 1);
        oreType = eligible[idx].blockType;
        return true;
    }

    public int ComputeTerrainHp(int depth, TerrainSpawnEntry terrain)
    {
        var hp = GetBaseHp(depth) * (terrain != null ? terrain.hpMultiplier : 1f);
        return Mathf.Max(1, Mathf.RoundToInt(hp));
    }

    public int ComputeTerrainSellPrice(BlockConfig cfg, TerrainSpawnEntry terrain)
    {
        // Обычная порода (sand/ground/stone/...) не растёт в цене от глубины.
        // Чтобы темп экономики был похож на "как раньше", используем базовую цену из BlockConfig.
        var basePrice = cfg != null ? cfg.baseSellPrice : baseSandSellPrice;
        var price = basePrice * (terrain != null ? terrain.sellPriceMultiplier : 1f);
        return Mathf.Max(1, Mathf.RoundToInt(price));
    }

    public int ComputeOreHp(int depth)
    {
        var hp = GetBaseHp(depth) * oreHpMultiplierVsDepthBase;
        return Mathf.Max(1, Mathf.RoundToInt(hp));
    }

    public int ComputeOreSellPrice(int depth, OreSpawnEntry ore)
    {
        var mult = ore != null ? ore.sellPriceMultiplier : 1f;
        var price = GetOreBaseSellPrice(depth) * mult;
        return Mathf.Max(1, Mathf.RoundToInt(price));
    }
}

[Serializable]
public class TerrainSpawnEntry
{
    public BlockType blockType;
    public int minDepth;
    [Tooltip("0 = без верхней границы")]
    public int maxDepth;
    public float depthCenter;
    public float hpMultiplier = 1f;
    public float sellPriceMultiplier = 1f;
}

[Serializable]
public class OreSpawnEntry
{
    public BlockType blockType;
    public int minDepth;
    public float sellPriceMultiplier = 3f;
}

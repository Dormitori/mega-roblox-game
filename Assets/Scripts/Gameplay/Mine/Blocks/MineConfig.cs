using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Gameplay/MineConfig")]
public class MineConfig : ScriptableObject
{
    public List<Block> blockPrefabs;
    public List<MineLevelsConfig> mineLevelsConfig;
    public ChestSpawnData chestSpawns;
    public int mineSize;

    public int yBlockSize;
    public int xBlockSize;
    public int zBlockSize;
}

[Serializable]
public class ChestSpawnData
{
    public bool enabled = true;
    [Tooltip("Сундуки не спавнятся на этой глубине и ниже (0 = только стартовый ряд без сундуков).")]
    public int minDepth = 1;
    [Tooltip("Вероятность на один блок (~0.012 ≈ в среднем 1 сундук на 2–3 уровня при сетке 6×36).")]
    [Range(0f, 1f)]
    public float chancePerBlock = 0.012f;
    public ChestSpawnEntry[] entries = System.Array.Empty<ChestSpawnEntry>();
}

[Serializable]
public class ChestSpawnEntry
{
    public BlockType blockType;
    public int weight;
}

[Serializable]
public class MineLevelsConfig
{
    public int startLevel;
    public int endLevel;
    public List<BlockProbability> blockProbabilities;
}

[Serializable]
public class BlockProbability
{
    public BlockType blockType;
    public int probability;
    public int minVariant = 1;
    public int maxVariant = 5;
}
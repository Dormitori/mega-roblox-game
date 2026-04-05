using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Gameplay/MineConfig")]
public class MineConfig : ScriptableObject
{
    public List<Block> blockPrefabs;
    public List<MineLevelsConfig> mineLevelsConfig;
    public int mineSize;

    public int yBlockSize;
    public int xBlockSize;
    public int zBlockSize;
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
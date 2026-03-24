using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

public class MineManager : MonoBehaviour
{
    public Transform blocksParent;
    public MineConfig mineConfig;

    private MineConfig _config;
    private Dictionary<string, ObjectPool<Block>> _blocksPools = new();
    private IInventory _inventory;

    private List<Quaternion> _cubeRotations = new();
    private int _curBlocks = 0;
    private int _maxBlocksPerLevel = 100;

    private int _currentLevelDestroyedBlocks;
    private int _currentLevelBlocksCount;
    private int _currentDeepLevel;
    private List<Block> _nextLevelBlocks;

    [Inject]
    public void Initialize(IInventory inventory)
    {
        _inventory = inventory;
        _config = mineConfig;
        _cubeRotations = GetUpwardRotations();
        foreach (var block in _config.blockPrefabs)
        {
            _blocksPools[block.name] = new ObjectPool<Block>(block, blocksParent);
        }
    }

    private void Start()
    {
        _currentLevelBlocksCount = GenerateMineLevel().Count;
        _nextLevelBlocks = GenerateMineLevel();
        DisableBlocks(_nextLevelBlocks);
    }

    private List<Block> GenerateMineLevel()
    {
        List<Block> blocks = new();
        for (var i = 0; i < _config.xBlockSize * _config.mineSize; i += _config.xBlockSize)
        for (var j = 0; j < _config.yBlockSize * _config.mineSize; j += _config.yBlockSize)
        {
            var blockName = GetBlockName(_currentDeepLevel);
            var blockPool = _blocksPools[blockName];
            var block = blockPool.Rent(false);
            blocks.Add(block);
            block.BlockDestroyed += OnBlockDestroy;
            block.transform.localPosition = new Vector3(i, -_currentDeepLevel * _config.zBlockSize, j);
            block.transform.rotation = _cubeRotations[Random.Range(0, _cubeRotations.Count)];
            block.gameObject.SetActive(true);
            _curBlocks++;
            if (_curBlocks >= _maxBlocksPerLevel) // защита от бесконечного цикла
                return blocks;
        }

        _curBlocks = 0;
        _currentDeepLevel++;
        return blocks;
    }

    private void OnBlockDestroy(Block block)
    {
        _inventory.AddItem(block.config.item, 1);
        _currentLevelDestroyedBlocks++;
        block.ResetHealth();
        block.BlockDestroyed -= OnBlockDestroy;
        block.Disable();
        _blocksPools[block.name].Return(block);
        if (_currentLevelDestroyedBlocks >= _currentLevelBlocksCount)
        {
            EnableBlocks(_nextLevelBlocks);
            _currentLevelBlocksCount = _nextLevelBlocks.Count;
            _nextLevelBlocks.Clear();
            _nextLevelBlocks = GenerateMineLevel();
            DisableBlocks(_nextLevelBlocks);
            _currentLevelDestroyedBlocks = 0;
        }
    }

    private string GetBlockName(int deepLevel)
    {
        var levelsConfig = _config.mineLevelsConfig;
        foreach (var levelConfig in levelsConfig)
        {
            if (deepLevel >= levelConfig.startLevel && deepLevel <= levelConfig.endLevel)
            {
                var probabilities = levelConfig.blockProbabilities.Select(x => x.probability).ToList();
                var names = levelConfig.blockProbabilities.Select(x => x.blockName).ToList();
                return RandomUtils.WeightedRandom(probabilities, names);
            }
        }

        throw new System.Exception($"Level {deepLevel} not found");
    }


    private List<Quaternion> GetUpwardRotations()
    {
        float[] angles = { 0f, 90f, 180f, 270f };
        return angles.Select(angle => Quaternion.Euler(0, angle, 0)).ToList();
    }

    private void DisableBlocks(List<Block> blocks)
    {
        foreach (var block in blocks)
            block.Disable();
    }

    private void EnableBlocks(List<Block> blocks)
    {
        foreach (var block in blocks)
            block.Enable();
    }
}
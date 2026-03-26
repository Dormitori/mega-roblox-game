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
    private Dictionary<Items, Dictionary<int, ObjectPool<Block>>> _blocksPools = new();
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
            if (!_blocksPools.ContainsKey(block.blockType))
            {
                _blocksPools[block.blockType] = new Dictionary<int, ObjectPool<Block>>();
            }
            
            _blocksPools[block.blockType][block.variantId] = new ObjectPool<Block>(block, blocksParent);
            Debug.Log($"Created pool for {block.blockType} variant {block.variantId}");
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
        var blocks = new List<Block>();
        for (var i = 0; i < _config.xBlockSize * _config.mineSize; i += _config.xBlockSize)
        {
            for (var j = 0; j < _config.yBlockSize * _config.mineSize; j += _config.yBlockSize)
            {
                var (blockType, variantId) = GetBlockInfo(_currentDeepLevel);
                var blockPool = _blocksPools[blockType][variantId];
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
        _blocksPools[block.blockType][block.variantId].Return(block);
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

    private (Items blockType, int variantId) GetBlockInfo(int deepLevel)
    {
        var levelsConfig = _config.mineLevelsConfig;
        foreach (var levelConfig in levelsConfig)
        {
            if (deepLevel >= levelConfig.startLevel && deepLevel <= levelConfig.endLevel)
            {
                var probabilities = levelConfig.blockProbabilities.Select(x => x.probability).ToList();
                var blockTypes = levelConfig.blockProbabilities.Select(x => (int)x.blockType).ToList();
                int selectedIndex = RandomUtils.WeightedRandom(probabilities, blockTypes);
                var selectedBlock = levelConfig.blockProbabilities[selectedIndex];
                
                int variantId = Random.Range(selectedBlock.minVariant, selectedBlock.maxVariant + 1);
                return (selectedBlock.blockType, variantId);
            }
        }

        throw new System.Exception($"Level {deepLevel} not found");
    }

    private void SetupBlockPrefab(Block block)
    {
        // Проверяем и настраиваем необходимые компоненты
        if (block.GetComponent<Health>() == null)
        {
            Debug.LogWarning($"Adding Health component to {block.gameObject.name}");
            block.gameObject.AddComponent<Health>();
        }
        
        if (block.GetComponent<MeshRenderer>() == null)
        {
            Debug.LogWarning($"No MeshRenderer found on {block.gameObject.name}");
        }
        
        // Устанавливаем анимационную конфигурацию по умолчанию если не установлена
        if (block.animationConfig == null)
        {
            var defaultAnimConfig = Resources.Load<BlockAnimationConfig>("DefaultBlockAnimationConfig");
            if (defaultAnimConfig != null)
            {
                block.animationConfig = defaultAnimConfig;
                Debug.Log($"Set default animation config for {block.blockType} variant {block.variantId}");
            }
            else
            {
                Debug.LogWarning($"No default animation config found. Create 'DefaultBlockAnimationConfig' in Resources folder.");
            }
        }
        
        // Если config не установлен, пытаемся найти подходящий
        if (block.config == null)
        {
            Debug.LogWarning($"Block {block.blockType} variant {block.variantId} has no config assigned. Looking for config...");
            
            // Ищем конфигурацию по типу блока
            var config = FindBlockConfig(block.blockType);
            if (config != null)
            {
                block.config = config;
                Debug.Log($"Assigned config {config.name} to block {block.blockType} variant {block.variantId}");
            }
            else
            {
                Debug.LogError($"No config found for block {block.blockType}. Creating default config.");
                CreateDefaultConfig(block);
            }
        }
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
    
    private BlockConfig FindBlockConfig(Items itemType)
    {
        // Ищем в ресурсах конфигурацию по типу предмета
        var configs = Resources.LoadAll<BlockConfig>("Configs");
        
        foreach (var config in configs)
        {
            if (config.item == itemType)
            {
                return config;
            }
        }
        
        return null;
    }
    
    private void CreateDefaultConfig(Block block)
    {
        // Создаем временную конфигурацию по умолчанию
        var defaultConfig = ScriptableObject.CreateInstance<BlockConfig>();
        defaultConfig.item = block.blockType;
        defaultConfig.name = $"{block.blockType}_{block.variantId}";
        defaultConfig.health = 100f; // Значение по умолчанию
        
        block.config = defaultConfig;
        
        Debug.Log($"Created default config for block {block.blockType} variant {block.variantId} with health {defaultConfig.health}");
    }
}
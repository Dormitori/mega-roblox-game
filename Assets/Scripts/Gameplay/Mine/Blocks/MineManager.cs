using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = System.Object;
using Random = UnityEngine.Random;

public class MineManager : MonoBehaviour
{
    public Transform blocksParent;
    public MineConfig mineConfig;
    public ParticleSystem destroyParticles;
    public float destroyParticlesDuration;
    public BlockHpPopup hpPopupPrefab;

    private MineConfig _config;
    private Dictionary<BlockType, Dictionary<int, ObjectPool<Block>>> _blocksPools = new();
    private ObjectPool<ParticleSystem> _destroyParticlesPool;
    private ObjectPool<BlockHpPopup> _hpPopupPool;
    private Inventory _inventory;

    private List<Quaternion> _cubeRotations = new();
    private int _curBlocks = 0;
    private int _maxBlocksPerLevel = 100;

    private int _currentLevelDestroyedBlocks;
    private int _currentLevelBlocksCount;
    private int _currentDeepLevel;
    private List<Block> _nextLevelBlocks;

    [Inject]
    public void Initialize(Inventory inventory)
    {
        _inventory = inventory;
        _config = mineConfig;
        _cubeRotations = GetUpwardRotations();
        _destroyParticlesPool = new ObjectPool<ParticleSystem>(destroyParticles, blocksParent);

        _hpPopupPool = new ObjectPool<BlockHpPopup>(hpPopupPrefab, blocksParent, prewarm: 4);

        foreach (var block in _config.blockPrefabs)
        {
            if (!_blocksPools.ContainsKey(block.blockType))
            {
                _blocksPools[block.blockType] = new Dictionary<int, ObjectPool<Block>>();
            }

            _blocksPools[block.blockType][block.variantId] = new ObjectPool<Block>(block, blocksParent);
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
                block.Damaged += OnBlockDamaged;
                block.transform.localPosition = new Vector3(i, -_currentDeepLevel * _config.zBlockSize, j);
                block.transform.rotation = _cubeRotations[Random.Range(0, _cubeRotations.Count)];
                block.gameObject.SetActive(true);
                _curBlocks++;
                if (_curBlocks >= _maxBlocksPerLevel)
                    return blocks;
            }
        }

        _curBlocks = 0;
        _currentDeepLevel++;
        return blocks;
    }

    private void OnBlockDamaged(Block block, int remaining, int maxHp)
    {
        var popup = _hpPopupPool.Rent();
        if (popup == null) return;
        popup.Play(remaining, maxHp, block.transform.position, () => _hpPopupPool.Return(popup));
    }

    private void OnBlockDestroy(Block block)
    {
        block.Damaged -= OnBlockDamaged;
        _inventory.AddBlock(block.config.type, 1);
        _currentLevelDestroyedBlocks++;
        block.ResetHealth();

        if (destroyParticles != null)
        {
            StartCoroutine(PlayAndKillParticle(block.transform.position, block.destroyParticlesMaterial));
        }

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

    private IEnumerator PlayAndKillParticle(Vector3 blockPosition, Material blockMaterial)
    {
        var particle = _destroyParticlesPool.Rent();
        particle.GetComponent<ParticleSystemRenderer>().sharedMaterial = blockMaterial;

        particle.transform.position = blockPosition;
        particle.Play();
        yield return new WaitForSeconds(destroyParticlesDuration);
        particle.Stop();
        _destroyParticlesPool.Return(particle);
    }

    private (BlockType blockType, int variantId) GetBlockInfo(int deepLevel)
    {
        var levelsConfig = _config.mineLevelsConfig;
        foreach (var levelConfig in levelsConfig)
        {
            if (deepLevel >= levelConfig.startLevel && deepLevel <= levelConfig.endLevel)
            {
                var probabilities = levelConfig.blockProbabilities.Select(x => x.probability).ToList();
                var blockTypes = levelConfig.blockProbabilities.Select(x => x.blockType).ToList();

                var selectedBlock = RandomUtils.WeightedRandom(probabilities, blockTypes);
                foreach (var config in levelConfig.blockProbabilities)
                {
                    if (config.blockType != selectedBlock) continue;
                    var variantId = Random.Range(config.minVariant, config.maxVariant + 1);
                    return (selectedBlock, variantId);
                }
            }
        }

        throw new Exception($"Level {deepLevel} not found");
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Audio;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

public class MineManager : MonoBehaviour
{
    public List<Vector2> BlocksGridPositions { get; } = new();

    public Transform blocksParent;
    public MineConfig mineConfig;

    [Tooltip("Если не задан — используется старая генерация по mineLevelsConfig")]
    public MineBalanceConfig balanceConfig;

    public ParticleSystem destroyParticles;
    public float destroyParticlesDuration;
    public BlockHpPopup hpPopupPrefab;

    private MineConfig _config;
    private ConfigManager<BlockConfig> _blockConfigs;
    private Dictionary<BlockType, Dictionary<int, ObjectPool<Block>>> _blocksPools = new();
    private ObjectPool<ParticleSystem> _destroyParticlesPool;
    private ObjectPool<BlockHpPopup> _hpPopupPool;
    private Inventory _inventory;
    private IAudioService _audioService;
    private ISaveService _saveService;
    private MineLootToastController _lootToast;

    private List<Quaternion> _cubeRotations = new();
    private int _curBlocks = 0;
    private int _maxBlocksPerLevel = 100;

    private int _currentLevelDestroyedBlocks;
    private int _currentLevelBlocksCount;
    private int _currentGeneratedDeepLevel;
    private int _currentDeepLevel;
    private List<Block> _nextLevelBlocks;


    [Inject]
    public void Initialize(Inventory inventory, IAudioService audioService, ISaveService saveService,
        ConfigManager<BlockConfig> blockConfigs,
        MineLootToastController lootToast)
    {
        _inventory = inventory;
        _audioService = audioService;
        _saveService = saveService;
        _blockConfigs = blockConfigs;
        _lootToast = lootToast;
        _config = mineConfig;
        _cubeRotations = GetUpwardRotations();
        _destroyParticlesPool = new ObjectPool<ParticleSystem>(destroyParticles, blocksParent);

        _hpPopupPool = new ObjectPool<BlockHpPopup>(hpPopupPrefab, blocksParent, prewarm: 4);

        if (saveService.HasKey(SaveKeys.MineDeepLevel))
            _currentDeepLevel = saveService.Load<int>(SaveKeys.MineDeepLevel);

        _currentGeneratedDeepLevel = _currentDeepLevel;

        SaveTrigger.OnSave += SaveCurrentDeepLevel;

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
        foreach (var block in _nextLevelBlocks)
            BlocksGridPositions.Add(new Vector2(block.transform.position.x, block.transform.position.z));
        DisableBlocks(_nextLevelBlocks);
    }

    private List<Block> GenerateMineLevel()
    {
        var blocks = new List<Block>();
        for (var i = 0; i < _config.xBlockSize * _config.mineSize; i += _config.xBlockSize)
        {
            for (var j = 0; j < _config.yBlockSize * _config.mineSize; j += _config.yBlockSize)
            {
                var (logicalType, visualType, variantId, hp, sellPrice, invConfig) =
                    ResolveBlockSpawn(_currentGeneratedDeepLevel);
                var blockPool = _blocksPools[visualType][variantId];
                var block = blockPool.Rent(false);
                blocks.Add(block);
                block.BlockDestroyed += OnBlockDestroy;
                block.Damaged += OnBlockDamaged;
                block.ApplyMineSpawn(invConfig, hp, sellPrice);
                block.transform.localPosition = new Vector3(i, -_currentGeneratedDeepLevel * _config.zBlockSize, j);
                block.transform.rotation = _cubeRotations[Random.Range(0, _cubeRotations.Count)];
                block.gameObject.SetActive(true);
                _curBlocks++;
                if (_curBlocks >= _maxBlocksPerLevel)
                    return blocks;
            }
        }

        _curBlocks = 0;
        _currentGeneratedDeepLevel++;
        return blocks;
    }

    private void OnBlockDamaged(Block block, int remaining, int maxHp)
    {
        if (_hpPopupPool == null) return;
        var popup = _hpPopupPool.Rent();
        if (popup == null) return;
        popup.Play(remaining, maxHp, block.transform.position, () => _hpPopupPool.Return(popup));
    }

    private void OnBlockDestroy(Block block)
    {
        block.Damaged -= OnBlockDamaged;
        _audioService?.PlaySfx(SoundId.BlockDestroy, 1f, 0.5f, 1.1f);
        if (MineChestRules.GrantsCrystalsOnly(block.InventoryBlockType))
        {
            _inventory.AddCurrency(CurrencyType.Crystals, 1);
            _lootToast?.ShowText($"+1 {I2.Loc.LocalizationManager.GetTranslation("Crystals")}", null);
        }
        else
        {
            _inventory.AddBlock(block.InventoryBlockType, 1, block.RuntimeSellPrice);
            _lootToast?.ShowBlock(ConfigFor(block.InventoryBlockType), 1);
        }

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
            _currentDeepLevel++;
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

    private BlockConfig ConfigFor(BlockType type) =>
        _blockConfigs.Configs.FirstOrDefault(c => c.type == type);

    private (BlockType logicalType, BlockType visualType, int variantId, int hp, int sellPrice, BlockConfig invConfig)
        ResolveBlockSpawn(int deepLevel)
    {
        if (TryResolveChestSpawn(deepLevel,
                out var cl, out var cv, out var cvar, out var chp, out var cprice, out var ccfg))
            return (cl, cv, cvar, chp, cprice, ccfg);

        if (balanceConfig != null)
            return ResolveBlockSpawnBalanced(deepLevel);

        return ResolveBlockSpawnLegacy(deepLevel);
    }

    private bool TryResolveChestSpawn(int deepLevel,
        out BlockType logical, out BlockType visual, out int variantId, out int hp, out int sellPrice,
        out BlockConfig invConfig)
    {
        logical = default;
        visual = default;
        variantId = 1;
        hp = 1;
        sellPrice = 0;
        invConfig = null;

        var cs = _config.chestSpawns;
        if (cs == null || !cs.enabled || deepLevel < cs.minDepth || cs.entries == null || cs.entries.Length == 0)
            return false;
        if (Random.value >= cs.chancePerBlock)
            return false;

        var weights = new List<int>();
        var types = new List<BlockType>();
        foreach (var e in cs.entries)
        {
            if (e.weight <= 0)
                continue;
            var cfg = ConfigFor(e.blockType);
            if (cfg == null)
                continue;
            weights.Add(e.weight);
            types.Add(e.blockType);
        }

        if (types.Count == 0)
            return false;

        logical = RandomUtils.WeightedRandom(weights, types);
        invConfig = ConfigFor(logical);
        if (invConfig == null)
            return false;

        hp = Mathf.Max(1, Mathf.RoundToInt(invConfig.health));
        sellPrice = invConfig.baseSellPrice;
        visual = ResolveVisualBlockType(logical);
        variantId = RandomVariantFor(visual);
        return true;
    }

    private (BlockType logicalType, BlockType visualType, int variantId, int hp, int sellPrice, BlockConfig invConfig)
        ResolveBlockSpawnBalanced(int deepLevel)
    {
        var isOre = false;
        BlockType logical = default;

        if (Random.value < balanceConfig.GetOreChance(deepLevel) &&
            balanceConfig.TryPickOre(deepLevel, () => Random.value, t => ConfigFor(t) != null, out var orePick))
        {
            logical = orePick;
            isOre = true;
        }

        if (!isOre)
            logical = balanceConfig.PickTerrain(deepLevel, () => Random.value);

        var oreEntry = isOre ? balanceConfig.GetOreEntry(logical) : null;
        if (isOre && oreEntry == null)
        {
            isOre = false;
            logical = balanceConfig.PickTerrain(deepLevel, () => Random.value);
        }

        var terrainEntry = !isOre ? balanceConfig.GetTerrainEntry(logical) : null;

        var cfg = ConfigFor(logical);
        if (cfg == null)
        {
            logical = BlockType.Sand;
            terrainEntry = balanceConfig.GetTerrainEntry(logical);
            cfg = ConfigFor(logical);
        }

        if (cfg == null)
            throw new InvalidOperationException(
                "Не найден BlockConfig для базового блока (Sand). Добавьте ассеты в Resources/BlockConfig.");

        int hp;
        int price;
        if (isOre && oreEntry != null)
        {
            hp = balanceConfig.ComputeOreHp(deepLevel);
            price = balanceConfig.ComputeOreSellPrice(deepLevel, oreEntry);
        }
        else
        {
            hp = balanceConfig.ComputeTerrainHp(deepLevel, terrainEntry);
            price = balanceConfig.ComputeTerrainSellPrice(deepLevel, terrainEntry);
        }

        var visual = ResolveVisualBlockType(logical);
        var variantId = RandomVariantFor(visual);
        return (logical, visual, variantId, hp, price, cfg);
    }

    private (BlockType logicalType, BlockType visualType, int variantId, int hp, int sellPrice, BlockConfig invConfig)
        ResolveBlockSpawnLegacy(int deepLevel)
    {
        var levelsConfig = _config.mineLevelsConfig;
        foreach (var levelConfig in levelsConfig)
        {
            if (deepLevel >= levelConfig.startLevel && deepLevel <= levelConfig.endLevel)
            {
                var probabilities = levelConfig.blockProbabilities.Select(x => x.probability).ToList();
                var blockTypes = levelConfig.blockProbabilities.Select(x => x.blockType).ToList();

                var selectedBlock = RandomUtils.WeightedRandom(probabilities, blockTypes);
                foreach (var bp in levelConfig.blockProbabilities)
                {
                    if (bp.blockType != selectedBlock) continue;
                    var variantId = Random.Range(bp.minVariant, bp.maxVariant + 1);
                    var cfg = ConfigFor(selectedBlock);
                    if (cfg == null)
                        throw new Exception($"BlockConfig for {selectedBlock} not found");
                    var hp = Mathf.Max(1, Mathf.RoundToInt(cfg.health));
                    return (selectedBlock, selectedBlock, variantId, hp, cfg.baseSellPrice, cfg);
                }
            }
        }

        throw new Exception($"Level {deepLevel} not found");
    }

    private BlockType ResolveVisualBlockType(BlockType logical)
    {
        if (_blocksPools.ContainsKey(logical) && _blocksPools[logical].Count > 0)
            return logical;

        if (balanceConfig != null)
        {
            var fb = balanceConfig.fallbackVisualBlockType;
            if (_blocksPools.ContainsKey(fb) && _blocksPools[fb].Count > 0)
                return fb;
        }

        return _blocksPools.Keys.First();
    }

    private int RandomVariantFor(BlockType visual)
    {
        if (!_blocksPools.TryGetValue(visual, out var byVariant) || byVariant.Count == 0)
            return 1;
        var keys = byVariant.Keys.ToList();
        return keys[Random.Range(0, keys.Count)];
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

    private void SaveCurrentDeepLevel()
    {
        _saveService.Save(SaveKeys.MineDeepLevel, _currentDeepLevel);
    }
}
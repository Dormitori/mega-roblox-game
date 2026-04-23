using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class Inventory
{
    public event Action CurrencyChanged;
    public event Action BlocksChanged;
    public event Action BackpackCapacityChanged;
    public event Action PickaxeBuy;
    public event Action PickaxeEquip;

    public PickaxeType CurrentPickaxe { get; private set; }
    public HashSet<BlockType> UnlockedBuyableBlocks { get; } = new(); 

    private Dictionary<CurrencyType, int> _currencies = new();
    private Dictionary<BlockType, int> _blocks = new();
    private Dictionary<BlockType, long> _blockSellValueTotals = new();
    private HashSet<PickaxeType> _pickaxes = new();
    
    private int _backpackCapacity = 40;

    private ISaveService _saveService;
    private ConfigManager<BlockConfig> _blockConfigs;
    
    private readonly HashSet<BlockType> _buyableBlocks = new()
    {
        BlockType.Sand, BlockType.Ground, BlockType.Stone, BlockType.Rock, BlockType.Lava, BlockType.Obsidian, 
        BlockType.OreCoal, BlockType.OreCopper, BlockType.OreIron, BlockType.OreBronze, BlockType.OreQuartz, 
        BlockType.OreSilver, BlockType.OreGold, BlockType.OreLapis, BlockType.OreDiamond, BlockType.OreAmethyst, 
        BlockType.OreGreenDiamond, BlockType.OreRuby,
    };
    
    [Inject]
    public Inventory(ISaveService saveService, ConfigManager<BlockConfig> blockConfigs)
    {
        _saveService = saveService;
        _blockConfigs = blockConfigs;
        SaveTrigger.OnSave += Save;
        
        if (saveService.HasKey(SaveKeys.Inventory))
        {
            var saveData = saveService.Load<InventorySaveData>(SaveKeys.Inventory);
            _currencies = saveData.Currencies;
            _blocks = saveData.Blocks;
            _pickaxes = saveData.Pickaxes;
            SetBackpackCapacity(saveData.backpackCapacity);
            CurrentPickaxe = saveData.CurrentPickaxe;
            EnsureAllBlockTypesPresent(_blocks);
            _blockSellValueTotals = saveData.BlockSellValueTotals ?? new Dictionary<BlockType, long>();
            EnsureAllBlockTypesPresent(_blockSellValueTotals);
            if (saveData.UnlockedBuyableBlocks != null)
                UnlockedBuyableBlocks = saveData.UnlockedBuyableBlocks;
            if (saveData.BlockSellValueTotals == null)
                MigrateSellTotalsFromBlockConfigs();
            EnsureAllCurrenciesPresent(_currencies);
            return;
        }
        
        foreach (var item in Enum.GetValues(typeof(CurrencyType)))
            _currencies.Add((CurrencyType)item, 0);

        foreach (var item in Enum.GetValues(typeof(BlockType)))
        {
            _blocks.Add((BlockType)item, 0);
            _blockSellValueTotals.Add((BlockType)item, 0);
        }

        CurrentPickaxe = PickaxeType.PickaxeWood01;
        AddPickaxe(PickaxeType.PickaxeWood01);
        Save();
    }

    private static void EnsureAllCurrenciesPresent(Dictionary<CurrencyType, int> dict)
    {
        foreach (CurrencyType c in Enum.GetValues(typeof(CurrencyType)))
            if (!dict.ContainsKey(c))
                dict[c] = 0;
    }

    private static void EnsureAllBlockTypesPresent(Dictionary<BlockType, int> dict)
    {
        foreach (BlockType t in Enum.GetValues(typeof(BlockType)))
            if (!dict.ContainsKey(t))
                dict[t] = 0;
    }

    private static void EnsureAllBlockTypesPresent(Dictionary<BlockType, long> dict)
    {
        foreach (BlockType t in Enum.GetValues(typeof(BlockType)))
            if (!dict.ContainsKey(t))
                dict[t] = 0;
    }

    private void MigrateSellTotalsFromBlockConfigs()
    {
        foreach (BlockType t in Enum.GetValues(typeof(BlockType)))
        {
            var count = _blocks[t];
            if (count <= 0)
            {
                _blockSellValueTotals[t] = 0;
                continue;
            }

            var cfg = _blockConfigs.Configs.FirstOrDefault(c => c.type == t);
            var unit = cfg != null ? cfg.baseSellPrice : 0;
            _blockSellValueTotals[t] = (long)unit * count;
        }
    }

    public void AddCurrency(CurrencyType currency, int amount)
    {
        _currencies[currency] += amount;
        CurrencyChanged?.Invoke();
    }

    public bool TryRemoveCurrency(CurrencyType currency, int amount)
    {
        if (_currencies[currency] < amount)
            return false;

        _currencies[currency] -= amount;
        CurrencyChanged?.Invoke();
        return true;
    }

    public int GetCurrencyCount(CurrencyType currency)
    {
        return _currencies[currency];
    }


    public void AddBlock(BlockType block, int amount, int unitSellPrice)
    {
        if (_buyableBlocks.Contains(block))
            UnlockedBuyableBlocks.Add(block);

        _blocks[block] += amount;
        _blockSellValueTotals[block] += (long)unitSellPrice * amount;
        BlocksChanged?.Invoke();
    }

    public bool TryRemoveBlock(BlockType block, int amount)
    {
        if (_blocks[block] < amount)
            return false;

        var removedValue = _blockSellValueTotals[block] * amount / _blocks[block];
        _blocks[block] -= amount;
        _blockSellValueTotals[block] -= removedValue;
        BlocksChanged?.Invoke();
        return true;
    }

    public long GetSellValueTotal(BlockType block)
    {
        return _blockSellValueTotals[block];
    }

    public int GetBlockCount(BlockType block)
    {
        return _blocks[block];
    }

    public int GetAllBlockCount()
    {
        return _blocks.Values.Sum();
    }

    public int GetSpaceLeft()
    {
        return _backpackCapacity - GetAllBlockCount();
    }

    public void AddPickaxe(PickaxeType pickaxe)
    {
        _pickaxes.Add(pickaxe);
        PickaxeBuy?.Invoke();
    }

    public bool HasPickaxe(PickaxeType pickaxe)
    {
        return _pickaxes.Contains(pickaxe);
    }

    public void EquipPickaxe(PickaxeType pickaxe)
    {
        CurrentPickaxe = pickaxe;
        PickaxeEquip?.Invoke();
    }
    
    public void SetBackpackCapacity(int capacity)
    {
        _backpackCapacity = capacity;
        BackpackCapacityChanged?.Invoke();
    }

    public int GetBackpackCapacity()
    {
        return _backpackCapacity;
    }

    private void Save()
    {
        _saveService.Save(
            SaveKeys.Inventory, 
            new InventorySaveData(
                _currencies,
                _blocks,
                _blockSellValueTotals,
                _pickaxes,
                UnlockedBuyableBlocks,
                CurrentPickaxe,
                _backpackCapacity
            )
        );
    }
}

[Serializable]
public class InventorySaveData
{
    public Dictionary<CurrencyType, int> Currencies;
    public Dictionary<BlockType, int> Blocks;
    public Dictionary<BlockType, long> BlockSellValueTotals;
    public HashSet<PickaxeType> Pickaxes;
    public HashSet<BlockType> UnlockedBuyableBlocks;
    public PickaxeType CurrentPickaxe;
    public int backpackCapacity;

    public InventorySaveData(
        Dictionary<CurrencyType, int> currencies,
        Dictionary<BlockType, int> blocks,
        Dictionary<BlockType, long> blockSellValueTotals,
        HashSet<PickaxeType> pickaxes,
        HashSet<BlockType> unlockedBuyableBlocks,
        PickaxeType currentPickaxe,
        int backpackCapacity)
    {
        Currencies = currencies;
        Blocks = blocks;
        BlockSellValueTotals = blockSellValueTotals;
        Pickaxes = pickaxes;
        CurrentPickaxe = currentPickaxe;
        UnlockedBuyableBlocks = unlockedBuyableBlocks;
        this.backpackCapacity = backpackCapacity;
    }
}
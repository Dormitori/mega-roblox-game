using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

public class Inventory
{
    public event Action CurrencyChanged;
    public event Action<int> BlocksChanged;
    public event Action BackpackCapacityChanged;

    public PickaxeType CurrentPickaxe { get; private set; }

    private Dictionary<CurrencyType, int> _currencies = new();
    private Dictionary<BlockType, int> _blocks = new();
    private HashSet<PickaxeType> _pickaxes = new();

    
    private int _backpackCapacity = 10;

    private ISaveService _saveService;
    
    [Inject]
    public Inventory(ISaveService saveService)
    {
        _saveService = saveService;
        SaveTrigger.OnSave += Save;
        
        if (saveService.HasKey(SaveKeys.Inventory))
        {
            var saveData = saveService.Load<InventorySaveData>(SaveKeys.Inventory);
            _currencies = saveData.Currencies;
            _blocks = saveData.Blocks;
            _pickaxes = saveData.Pickaxes;
            SetBackpackCapacity(saveData.backpackCapacity);
            CurrentPickaxe = saveData.CurrentPickaxe;
            return;
        }
        
        foreach (var item in Enum.GetValues(typeof(CurrencyType)))
            _currencies.Add((CurrencyType)item, 0);

        foreach (var item in Enum.GetValues(typeof(BlockType)))
            _blocks.Add((BlockType)item, 0);

        CurrentPickaxe = PickaxeType.PickaxeWood01;
        AddPickaxe(PickaxeType.PickaxeWood01);
        Save();
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


    public void AddBlock(BlockType block, int amount)
    {
        _blocks[block] += amount;
        BlocksChanged?.Invoke(amount);
    }

    public bool TryRemoveBlock(BlockType block, int amount)
    {
        if (_blocks[block] < amount)
            return false;

        _blocks[block] -= amount;
        BlocksChanged?.Invoke(-amount);
        return true;
    }

    public int GetBlockCount(BlockType block)
    {
        return _blocks[block];
    }

    public int GetAllBlockCount()
    {
        return _blocks.Values.Sum();
    }

    public void AddPickaxe(PickaxeType pickaxe)
    {
        _pickaxes.Add(pickaxe);
    }

    public bool HasPickaxe(PickaxeType pickaxe)
    {
        return _pickaxes.Contains(pickaxe);
    }

    public void EquipPickaxe(PickaxeType pickaxe)
    {
        CurrentPickaxe = pickaxe;
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
                _pickaxes,
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
    public HashSet<PickaxeType> Pickaxes;
    public PickaxeType CurrentPickaxe;
    public int backpackCapacity;

    public InventorySaveData(Dictionary<CurrencyType, int> currencies, Dictionary<BlockType, int> blocks, HashSet<PickaxeType> pickaxes, PickaxeType currentPickaxe, int backpackCapacity)
    {
        Currencies = currencies;
        Blocks = blocks;
        Pickaxes = pickaxes;
        CurrentPickaxe = currentPickaxe;
        this.backpackCapacity = backpackCapacity;
    }
}
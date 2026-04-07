using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public event Action CurrencyChanged;
    public event Action<int> BlocksChanged;

    private Dictionary<CurrencyType, int> _currencies = new();
    private Dictionary<BlockType, int> _blocks = new();
    private HashSet<PickaxeType> _pickaxes = new();

    public Inventory()
    {
        foreach (var item in Enum.GetValues(typeof(CurrencyType)))
            _currencies.Add((CurrencyType)item, 0);

        foreach (var item in Enum.GetValues(typeof(BlockType)))
            _blocks.Add((BlockType)item, 0);
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
        Debug.Log(amount);
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

    public void AddPickaxe(PickaxeType pickaxe)
    {
        _pickaxes.Add(pickaxe);
    }

    public bool HasPickaxe(PickaxeType pickaxe)
    {
        return _pickaxes.Contains(pickaxe);
    }
}
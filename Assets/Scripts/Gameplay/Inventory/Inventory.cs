using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInventory
{
    event Action Changed;
    Dictionary<Items, int> ItemsCount { get; }
    void AddItem(Items item, int amount);
    bool TryRemoveItem(Items item, int amount);
    int GetItemCount(Items item);
}

public class Inventory : IInventory
{
    public event Action Changed;

    public Dictionary<Items, int> ItemsCount { get; private set; } = new();

    public Inventory()
    {
        foreach (var item in Enum.GetValues(typeof(Items)))
        {
            ItemsCount.Add((Items)item, 0);
        }

        ItemsCount[Items.PickaxeWood01] = 1;
    }

    public void AddItem(Items item, int amount)
    {
        ItemsCount[item] += amount;
        Changed?.Invoke();
    }

    public bool TryRemoveItem(Items item, int amount)
    {
        if (amount <= 0 || ItemsCount[item] - amount < 0) return false;
        ItemsCount[item] -= amount;
        return true;
    }

    public int GetItemCount(Items item)
    {
        return ItemsCount[item];
    }
}
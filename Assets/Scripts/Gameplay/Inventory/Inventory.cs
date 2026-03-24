using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInventory
{
    event Action Changed;
    Dictionary<Items, int> Items { get; }
    void AddItem(Items item, int amount);
    bool TryRemoveItem(Items item, int amount);
    int GetItemCount(Items item);
}

public class Inventory : IInventory
{
    public event Action Changed;

    public Dictionary<Items, int> Items { get; private set; } = new();

    public Inventory()
    {
        foreach (var item in Enum.GetValues(typeof(Items)))
        {
            Items.Add((Items)item, 0);
            Debug.Log(item.ToString());
        }
    }

    public void AddItem(Items item, int amount)
    {
        Items[item] += amount;
        Changed?.Invoke();
    }

    public bool TryRemoveItem(Items item, int amount)
    {
        if (amount <= 0 || Items[item] - amount < 0) return false;
        Items[item] -= amount;
        return true;
    }

    public int GetItemCount(Items item)
    {
        return Items[item];
    }
}
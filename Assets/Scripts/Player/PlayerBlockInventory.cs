using System;
using UnityEngine;
using VContainer;

public class PlayerBlockInventory : MonoBehaviour
{
    public event Action BlockCountChanged;
    public event Action CapacityChanged;
    
    public bool HasSpace => _currentCapacity > _currentBlockCount;
    public int CurrentCapacity => _currentCapacity;
    public int CurrentBlockCount => _currentBlockCount;
    
    public PopUpText notEnoughSpaceTextPrefab;
    public Transform textParent;
    
    private Inventory _inventory;
    private int _currentCapacity = 30;
    private int _currentBlockCount;
    
    
    [Inject]
    public void Initialize(Inventory inventory)
    {
        _inventory = inventory;
        inventory.BlocksChanged += OnBlocksChange;
        inventory.BackpackCapacityChanged += OnCapacityChange;
        _currentBlockCount = _inventory.GetAllBlockCount();
        OnCapacityChange();
    }

    private void OnCapacityChange()
    {
        _currentCapacity = _inventory.GetBackpackCapacity();
        CapacityChanged?.Invoke();
    }

    public void ShowNotEnoughSpaceText()
    {
        Instantiate(notEnoughSpaceTextPrefab, textParent);
    }
    
    private void OnBlocksChange(int amount)
    {
        _currentBlockCount += amount;
        BlockCountChanged?.Invoke();
    }
}

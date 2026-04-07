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
    

    private int _currentCapacity = 10;
    private int _currentBlockCount;
    
    [Inject]
    public void Initialize(Inventory inventory)
    {
        inventory.BlocksChanged += OnBlocksChange;
    }

    public void ShowNotEnoughSpaceText()
    {
        Instantiate(notEnoughSpaceTextPrefab, textParent);
    }

    public void SetCapacity(int capacity)
    {
        _currentCapacity = capacity;
        CapacityChanged?.Invoke();
    }
    
    private void OnBlocksChange(int amount)
    {
        _currentBlockCount += amount;
        print(_currentBlockCount);
        BlockCountChanged?.Invoke();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class PlayerPickaxe : MonoBehaviour
{
    public PickaxeConfig CurrentPickaxeConfig { get; private set; }
    
    public List<PickaxeModel> pickaxeModels;

    private GameObject _currentPickaxeModel;
    private List<PickaxeConfig> _pickaxeConfigs;

    private Inventory _inventory;
    
    [Inject]
    private void Initialize(Inventory inventory, ConfigManager<PickaxeConfig> configManager)
    {
        _inventory = inventory;
        _pickaxeConfigs = configManager.Configs;
        _currentPickaxeModel = pickaxeModels[0].pickaxeModel;
        EquipPickaxe(_inventory.CurrentPickaxe);
    }
    
    public void EquipPickaxe(PickaxeType type)
    {
        if (_currentPickaxeModel != null)
            _currentPickaxeModel.SetActive(false);
            
        foreach (var pickaxeModel in pickaxeModels)
        {
            if (pickaxeModel.item == type)
            {
                pickaxeModel.pickaxeModel.SetActive(true);
                _currentPickaxeModel = pickaxeModel.pickaxeModel;
            }
        }
        
        _inventory.EquipPickaxe(type);
        CurrentPickaxeConfig = _pickaxeConfigs.FirstOrDefault(c => c.type == type);
    }

    [Serializable]
    public class PickaxeModel
    {
        public PickaxeType item;
        public GameObject pickaxeModel;
    }
}
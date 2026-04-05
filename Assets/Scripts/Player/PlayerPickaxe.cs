using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickaxe : MonoBehaviour
{
    public PickaxeConfig CurrentPickaxeConfig { get; private set; }
    
    public PickaxeConfig defaultPickaxeConfig;
    public List<PickaxeModel> pickaxeModels;

    private GameObject _currentPickaxeModel;

    private void Awake()
    {
        EquipPickaxe(defaultPickaxeConfig);
    }

    public void EquipPickaxe(PickaxeConfig config)
    {
        if (_currentPickaxeModel != null)
            _currentPickaxeModel.SetActive(false);
            
        foreach (var pickaxeModel in pickaxeModels)
        {
            if (pickaxeModel.item == config.pickaxeType)
            {
                pickaxeModel.pickaxeModel.SetActive(true);
                _currentPickaxeModel = pickaxeModel.pickaxeModel;
            }
        }
        
        CurrentPickaxeConfig = config;
    }

    [Serializable]
    public class PickaxeModel
    {
        public PickaxeType item;
        public GameObject pickaxeModel;
    }
}
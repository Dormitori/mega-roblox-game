using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedPickaxeView : MonoBehaviour
{
    public event Action Buy;
    public event Action Equip;
    
    public TextMeshProUGUI pickaxeName;
    public TextMeshProUGUI pickaxeBuffs;
    public TextMeshProUGUI coinsText;
    public PickaxeRequirementsView pickaxeRequirementsView;
    public PickaxeShopView pickaxeShopView;
    public Button buyButton;
    public Button equipButton;
    public TextMeshProUGUI equippedText;

    private List<BlockConfig> _blockConfigs;
    private Inventory _inventory;
    
    private void Awake()
    {
        buyButton.onClick.AddListener(() => Buy?.Invoke());
        equipButton.onClick.AddListener(() => Equip?.Invoke());
        
        pickaxeShopView.UpdateView(true, false, false);
    }

    public void Initialize(List<BlockConfig> blockConfigs, Inventory inventory)
    {
        _blockConfigs = blockConfigs;
        _inventory = inventory;
    }

    public void UpdateView(
        PickaxeConfig pickaxeConfig,
        bool owned,
        bool equipped
    )
    {
        pickaxeName.text = pickaxeConfig.LocalizedName;
        pickaxeBuffs.text = MakeBuffList(pickaxeConfig);
        
        pickaxeShopView.UpdateIcon(pickaxeConfig.pickaxeIcon);
        
        if (!owned)
            pickaxeRequirementsView.SetRequirements(pickaxeConfig.RequirementsBlocks(_blockConfigs), _inventory);
        else
            pickaxeRequirementsView.Clear();
        coinsText.text = pickaxeConfig.price.ToString();
        
        if (equipped)
        {
            buyButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(false);
            equippedText.gameObject.SetActive(true);
        } else if (owned)
        {
            buyButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(true);
            equippedText.gameObject.SetActive(false);
        }
        else
        {
            buyButton.gameObject.SetActive(true);
            buyButton.interactable = pickaxeConfig.SatisfyRequirements(_inventory);
            equipButton.gameObject.SetActive(false);
            equippedText.gameObject.SetActive(false);
        }
    }

    private string MakeBuffList(PickaxeConfig pickaxeConfig)
    {
        return $"• <indent=10%>Pickaxe speed: {pickaxeConfig.baseSpeedMultiplier}</indent>";
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedPickaxeView : MonoBehaviour
{
    public event Action<PickaxeConfig, PickaxeShopView> Buy;
    public event Action<PickaxeConfig, PickaxeShopView> Equip;
    
    public TextMeshProUGUI PickaxeName;
    public TextMeshProUGUI PickaxeBuffs;
    public PickaxeShopView pickaxeShopView;
    public Button buyButton;
    public Button equipButton;
    public TextMeshProUGUI equippedText;

    private PickaxeConfig _pickaxeConfig;
    private PickaxeShopView _pickaxeShopView;
    
    private void Awake()
    {
        buyButton.onClick.AddListener(() => Buy?.Invoke(_pickaxeConfig, _pickaxeShopView));
        equipButton.onClick.AddListener(() => Equip?.Invoke(_pickaxeConfig, _pickaxeShopView));
    }

    public void SetSelected(
        PickaxeConfig pickaxeConfig, PickaxeShopView view, bool equipped, bool purchased, bool enoughMoneyToBuy = false
        )
    {
        PickaxeName.text = pickaxeConfig.pickaxeName;
        PickaxeBuffs.text = MakeBuffList(pickaxeConfig);
        pickaxeShopView.SetView(pickaxeConfig, true, false);
        _pickaxeConfig  = pickaxeConfig;
        _pickaxeShopView = view;
        if (equipped)
        {
            buyButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(false);
            equippedText.gameObject.SetActive(true);
        } else if (purchased)
        {
            buyButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(true);
            equippedText.gameObject.SetActive(false);
        }
        else
        {
            buyButton.gameObject.SetActive(true);
            buyButton.interactable = enoughMoneyToBuy;
            equipButton.gameObject.SetActive(false);
            equippedText.gameObject.SetActive(false);
        }
    }

    private string MakeBuffList(PickaxeConfig pickaxeConfig)
    {
        return $"• <indent=10%>Pickaxe speed: {pickaxeConfig.baseSpeedMultiplier}</indent>";
    }
}
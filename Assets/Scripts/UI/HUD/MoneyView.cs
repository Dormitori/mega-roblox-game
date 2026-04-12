using TMPro;
using UnityEngine;
using VContainer;

public class MoneyView : MonoBehaviour
{
    public TextMeshProUGUI CoinsText;
    [Tooltip("Опционально: счётчик кристаллов (донат-валюта).")]
    public TextMeshProUGUI CrystalsText;

    private Inventory _inventory;
    
    [Inject]
    public void Initialize(Inventory inventory)
    {
        inventory.CurrencyChanged += OnInventoryChanged;
        _inventory = inventory;
        OnInventoryChanged();
    }

    private void OnInventoryChanged()
    {
        CoinsText.text = _inventory.GetCurrencyCount(CurrencyType.Coins).ToString();
        if (CrystalsText != null)
            CrystalsText.text = _inventory.GetCurrencyCount(CurrencyType.Crystals).ToString();
    }
}
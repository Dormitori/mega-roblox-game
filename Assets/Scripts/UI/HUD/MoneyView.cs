using TMPro;
using UnityEngine;
using VContainer;

public class MoneyView : MonoBehaviour
{
    public TextMeshProUGUI CoinsText;

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
    }
}
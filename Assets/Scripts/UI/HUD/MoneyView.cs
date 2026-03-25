using TMPro;
using UnityEngine;
using VContainer;

public class MoneyView : MonoBehaviour
{
    public TextMeshProUGUI CoinsText;

    private IInventory _inventory;
    
    [Inject]
    public void Initialize(IInventory inventory)
    {
        inventory.Changed += OnInventoryChanged;
        _inventory = inventory;
        OnInventoryChanged();
    }

    private void OnInventoryChanged()
    {
        CoinsText.text = _inventory.GetItemCount(Items.Coins).ToString();
    }
}
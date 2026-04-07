using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class BlockInventoryView : MonoBehaviour
{
    public PlayerBlockInventory playerBlockInventory;
    
    private TextMeshProUGUI _text;

    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        playerBlockInventory.CapacityChanged += RefreshInventoryText;
        playerBlockInventory.BlockCountChanged += RefreshInventoryText;
        RefreshInventoryText();
    }

    private void RefreshInventoryText()
    {
        _text.text = $"{playerBlockInventory.CurrentBlockCount}/{playerBlockInventory.CurrentCapacity}";
    }
}

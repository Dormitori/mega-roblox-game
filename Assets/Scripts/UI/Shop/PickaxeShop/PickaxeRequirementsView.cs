using System.Collections.Generic;
using UnityEngine;

public class PickaxeRequirementsView : MonoBehaviour
{
    public PickaxeRequirementLine linePrefab;

    private List<PickaxeRequirementLine> lines = new();
    
    private readonly Color _satisfiedColor = Color.green;
    private readonly Color _notSatisfiedColor = Color.red;

    public void SetRequirements(IEnumerable<(BlockConfig, int)> requirements, IInventory inventory)
    {
        Clear();
        foreach (var requirement in requirements)
        {
            var line = Instantiate(linePrefab, transform);
            line.icon.sprite = requirement.Item1.icon;
            line.label.text = $"{inventory.ItemsCount[requirement.Item1.item]}/{requirement.Item2} {requirement.Item1.name}";
            if (inventory.ItemsCount[requirement.Item1.item] >= requirement.Item2)
                line.label.color  = _satisfiedColor;
            else
                line.label.color = _notSatisfiedColor;
            lines.Add(line);
        }
    }

    public void Clear()
    {
        foreach (var line in lines)
        {
            Destroy(line.gameObject);
        }
        lines.Clear();
    }
}
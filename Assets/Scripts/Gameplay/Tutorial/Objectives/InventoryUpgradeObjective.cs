using System;
using I2.Loc;

public class InventoryUpgradeObjective : ITutorialObjective
{
    public event Action ObjectiveComplete;
    public event Action ObjectiveUpdated;

    public InventoryUpgradeObjective(string objectiveText, Inventory inventory)
    {
        _objectiveText = objectiveText;
        _inventory = inventory;
    }

    private string _objectiveText;
    private Inventory _inventory;

    public string GetObjectiveText()
    {
        return "• " + LocalizationManager.GetTranslation(_objectiveText);
    }

    public void Activate()
    {
        _inventory.BackpackCapacityChanged += CompleteObjective;
    }

    public void Deactivate()
    {
        _inventory.BackpackCapacityChanged -= CompleteObjective;
    }

    private void CompleteObjective()
    {
        if (_inventory.GetBackpackCapacity() > 40)
            ObjectiveComplete?.Invoke();
    }
}
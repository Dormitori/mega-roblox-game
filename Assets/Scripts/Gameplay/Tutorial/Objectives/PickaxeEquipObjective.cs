using System;
using I2.Loc;

public class PickaxeEquipObjective : ITutorialObjective
{
    public event Action ObjectiveComplete;
    public event Action ObjectiveUpdated;

    private string _objectiveText;
    private Inventory _inventory;

    public PickaxeEquipObjective(string objectiveText, Inventory inventory)
    {
        _inventory = inventory;
        _objectiveText = objectiveText;
    }


    public string GetObjectiveText()
    {
        return "• " + LocalizationManager.GetTranslation(_objectiveText);
    }

    public void Activate() => _inventory.PickaxeEquip += CompleteObjective;

    public void Deactivate() => _inventory.PickaxeEquip -= CompleteObjective;

    private void CompleteObjective()
    {
        if (_inventory.CurrentPickaxe == PickaxeType.PickaxeStone02)
            ObjectiveComplete?.Invoke();
    }
}
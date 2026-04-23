using System;
using I2.Loc;

public class PickaxeObjective : ITutorialObjective
{
    public event Action ObjectiveComplete;
    public event Action ObjectiveUpdated;

    private string _objectiveText;
    private Inventory _inventory;

    public PickaxeObjective(string objectiveText, Inventory inventory)
    {
        _inventory = inventory;
        _objectiveText = objectiveText;
    }


    public string GetObjectiveText()
    {
        return "• " + LocalizationManager.GetTranslation(_objectiveText);
    }

    public void Activate() => _inventory.PickaxeBuy += CompleteObjective;

    public void Deactivate() => _inventory.PickaxeBuy -= CompleteObjective;

    private void CompleteObjective()
    {
        if (_inventory.HasPickaxe(PickaxeType.PickaxeStone02))
            ObjectiveComplete?.Invoke();
    }
}
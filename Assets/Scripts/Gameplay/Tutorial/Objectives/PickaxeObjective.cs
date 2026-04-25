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

    public void Activate()
    {
        TryCompleteObjective();
        _inventory.PickaxeBuy += TryCompleteObjective;
    }

    public void Deactivate() => _inventory.PickaxeBuy -= TryCompleteObjective;

    private void TryCompleteObjective()
    {
        if (_inventory.HasPickaxe(PickaxeType.PickaxeStone02))
            ObjectiveComplete?.Invoke();
    }
}
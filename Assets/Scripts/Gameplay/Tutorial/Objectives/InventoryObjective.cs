using System;
using System.Collections.Generic;
using I2.Loc;

public class InventoryObjective : ITutorialObjective
{
    public event Action ObjectiveComplete;
    public event Action ObjectiveUpdated;

    private Dictionary<BlockType, int> _blocksObjectives;
    private Dictionary<BlockType, string> _blockObjectiveMessages;

    private Dictionary<CurrencyType, int> _currencyObjectives;
    private Dictionary<CurrencyType, string> _currencyObjectiveMessages;

    private Inventory _inventory;

    public InventoryObjective(
        Dictionary<BlockType, string> blockObjectiveMessages, Dictionary<BlockType, int> blocksObjectives,
        Dictionary<CurrencyType, string> currencyObjectiveMessages, Dictionary<CurrencyType, int> currencyObjectives,
        Inventory inventory
    )
    {
        _currencyObjectiveMessages = currencyObjectiveMessages;
        _currencyObjectives = currencyObjectives;
        _blocksObjectives = blocksObjectives;
        _blockObjectiveMessages = blockObjectiveMessages;
        _inventory = inventory;
    }

    public string GetObjectiveText()
    {
        var objectiveText = "";

        foreach (var (currencyType, message) in _currencyObjectiveMessages)
            objectiveText += "• " + string.Format(LocalizationManager.GetTranslation(message), _inventory.GetCurrencyCount(currencyType)) + "\n";

        foreach (var (blockType, messsage) in _blockObjectiveMessages)
            objectiveText += "• " + string.Format(LocalizationManager.GetTranslation(messsage), _inventory.GetBlockCount(blockType)) + "\n";

        return objectiveText;
    }

    public void Activate()
    {
        _inventory.BlocksChanged += CheckObjective;
        _inventory.BlocksChanged += OnObjectiveUpdate;
        _inventory.CurrencyChanged += CheckObjective;
        _inventory.CurrencyChanged += OnObjectiveUpdate;
    }

    public void Deactivate()
    {
        _inventory.BlocksChanged -= CheckObjective;
        _inventory.BlocksChanged -= OnObjectiveUpdate;
        _inventory.CurrencyChanged -= CheckObjective;
        _inventory.CurrencyChanged -= OnObjectiveUpdate;
    }

    private void OnObjectiveUpdate()
    {
        ObjectiveUpdated?.Invoke();
    }

    private void CheckObjective()
    {
        foreach (var (blockType, count) in _blocksObjectives)
            if (_inventory.GetBlockCount(blockType) < count)
                return;

        foreach (var (currencyType, count) in _currencyObjectives)
            if (_inventory.GetCurrencyCount(currencyType) < count)
                return;

        ObjectiveComplete?.Invoke();
    }
}
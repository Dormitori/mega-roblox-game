using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class TutorialManager : MonoBehaviour
{
    public event Action TutorialCompleted;
    public event Action ObjectiveComplete;
    public event Action ObjectiveUpdate;

    public ITutorialObjective CurrentObjective { get; private set; }

    public List<ReachDestinationTrigger> ReachDestinationTriggers;

    public bool TutorialIsCompleted { get; private set; }

    private int _currentObjectiveIndex;
    private List<ITutorialObjective> _tutorialObjectives;
    private Inventory _inventory;
    private ISaveService _saveService;

    [Inject]
    public void Initialize(Inventory inventory, ISaveService saveService)
    {
        _inventory = inventory;
        _saveService = saveService;
        _tutorialObjectives = new()
        {
            new ReachDestinationObjective("Tutorial/ReachMine", ReachDestinationTriggers[0]),
            new InventoryObjective(
                new() { [BlockType.Sand] = "Tutorial/MineSand" }, new() { [BlockType.Sand] = 10 }, new(), new(),
                _inventory
            ),
            new ReachDestinationObjective("Tutorial/GoToShop", ReachDestinationTriggers[1]),
            new InventoryObjective(
                new(), new(),
                new() { [CurrencyType.Coins] = "Tutorial/SellSand" }, new() { [CurrencyType.Coins] = 20 },
                _inventory
            ),
            new InventoryUpgradeObjective("Tutorial/UpgradeInventory", inventory),
            new InventoryObjective(
                new()
                {
                    [BlockType.Sand] = "Tutorial/CollectSand",
                    [BlockType.OreCoal] = "Tutorial/CollectCoal"
                },
                new() { [BlockType.Sand] = 10, [BlockType.OreCoal] = 2 },
                new() { [CurrencyType.Coins] = "Tutorial/EarnCoins" },
                new() { [CurrencyType.Coins] = 100 }, _inventory
            ),
            new ReachDestinationObjective("Tutorial/GoToPickaxeShop", ReachDestinationTriggers[2]),
            new PickaxeObjective("Tutorial/BuyStonePickaxe", _inventory),
            new PickaxeEquipObjective("Tutorial/EquipStonePickaxe", _inventory)
        };

        if (CurrentObjective == null)
        {
            if (_saveService.HasKey(SaveKeys.CurrentTutorialStep))
            {
                _currentObjectiveIndex = _saveService.Load<int>(SaveKeys.CurrentTutorialStep);
                if (_currentObjectiveIndex >= _tutorialObjectives.Count)
                {
                    TutorialIsCompleted = true;
                    return;
                }
            }
            else
                _currentObjectiveIndex = 0;

            CurrentObjective = _tutorialObjectives[_currentObjectiveIndex];

            CurrentObjective.ObjectiveComplete += CompleteObjective;
            CurrentObjective.ObjectiveUpdated += UpdateObjective;
            CurrentObjective.Activate();
        }
    }

    private void CompleteObjective()
    {
        UpdateObjective();
        _currentObjectiveIndex++;
        CurrentObjective.ObjectiveComplete -= CompleteObjective;
        CurrentObjective.ObjectiveUpdated -= UpdateObjective;
        CurrentObjective.Deactivate();

        _saveService.Save(SaveKeys.CurrentTutorialStep, _currentObjectiveIndex);

        if (_currentObjectiveIndex >= _tutorialObjectives.Count)
        {
            TutorialIsCompleted = true;
            TutorialCompleted?.Invoke();
            return;
        }

        CurrentObjective = _tutorialObjectives[_currentObjectiveIndex];
        CurrentObjective.ObjectiveComplete += CompleteObjective;
        CurrentObjective.ObjectiveUpdated += UpdateObjective;

        ObjectiveComplete?.Invoke();
        CurrentObjective.Activate();
    }

    private void UpdateObjective()
    {
        ObjectiveUpdate?.Invoke();
    }
}
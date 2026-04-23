using System;
using I2.Loc;

public class ReachDestinationObjective : ITutorialObjective
{
    public event Action ObjectiveComplete;
    public event Action ObjectiveUpdated;

    private string _objectiveText;
    private ReachDestinationTrigger _reachDestinationTrigger;

    public ReachDestinationObjective(string objectiveText, ReachDestinationTrigger reachDestinationTrigger)
    {
        _objectiveText = objectiveText;
        _reachDestinationTrigger = reachDestinationTrigger;
    }

    public string GetObjectiveText()
    {
        return "• " + LocalizationManager.GetTranslation(_objectiveText);
    }

    public void Activate()
    {
        _reachDestinationTrigger.Reached += CompleteObjective;
        _reachDestinationTrigger.ShowFloatingArrow();
    }

    public void Deactivate()
    {
        _reachDestinationTrigger.Reached -= CompleteObjective;
        _reachDestinationTrigger.HideFloatingArrow();
    }

    private void CompleteObjective()
    {
        ObjectiveComplete?.Invoke();
    }
}
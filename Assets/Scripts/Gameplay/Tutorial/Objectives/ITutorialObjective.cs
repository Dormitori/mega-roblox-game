using System;

public interface ITutorialObjective
{
    public event Action ObjectiveComplete;
    public event Action ObjectiveUpdated;

    public string GetObjectiveText();
    public void Activate();
    public void Deactivate();
}
using System;

/// <summary>
/// Счётчик пересечений зон «внутри шахты». Несколько триггеров или один — поведение одинаковое.
/// </summary>
public static class CompanionMineState
{
    public static int OverlapCount { get; private set; }

    public static event Action<int> OverlapCountChanged;

    public static void Enter()
    {
        OverlapCount++;
        OverlapCountChanged?.Invoke(OverlapCount);
    }

    public static void Exit()
    {
        OverlapCount = Math.Max(0, OverlapCount - 1);
        OverlapCountChanged?.Invoke(OverlapCount);
    }
}

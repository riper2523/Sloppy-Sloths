using System;

public static class GameEvents
{
    public static Action OnFinishLineCrossed;
    public static Action<int> OnStarCollected;
    public static Action OnPlayStarted;
    public static Action<bool[]> OnLevelWon;
    public static Action OnBuild;
    public static Action OnRestartLevel;
}

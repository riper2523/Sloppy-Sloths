using System.Collections.Generic;
using Assets.Prefabs.LevelSystem.StarManager;

public struct LevelResult
{
    public List<StarResult> starResults;
    public List<int> collectedStarIDs;
    public float completionTime;
}

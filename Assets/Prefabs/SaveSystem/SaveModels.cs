using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public long lastPlayedUnixTime;
}

[System.Serializable]
public class LevelSaveData
{
    public string levelID;
    public List<int> collectedStarIDs = new List<int>();
    public List<int> achievedGoalIndices = new List<int>();
    public float bestTime = -1f; // -1 means no time recorded
    public GridSaveData lastAttemptedGrid = new GridSaveData();
}

[System.Serializable]
public class GridSaveData
{
    public List<CellSaveData> cells = new List<CellSaveData>();
}

[System.Serializable]
public class CellSaveData
{
    public int x;
    public int y;
    public List<PartSaveData> parts = new List<PartSaveData>();
}

[System.Serializable]
public class PartSaveData
{
    public string partID;
    public int layer;
    public int rotation;
}

using System.Collections.Generic;
using UnityEngine;

public enum StarGoalType
{
    FinishLevel,
    CollectStar,
    TimeLimit,
    NoDamage
}
[System.Serializable]
public struct StarGoal
{
    public StarGoalType goalType;
    public float timeLimit; // Only used if goalType is TimeLimit
}

public struct StarResult
{
    public StarGoalType goalType;
    public bool achieved;
}

public struct LevelResult
{
    public List<StarResult> starResults;
    public float completionTime;
}

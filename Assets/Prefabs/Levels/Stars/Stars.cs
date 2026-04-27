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

using UnityEngine;

namespace Assets.Prefabs.LevelSystem.StarManager
{
    public enum StarGoalType
    {
        FinishLevel,
        CollectStar,
        TimeLimit
    }

    [System.Serializable]
    public struct StarGoal
    {
        public StarGoalType goalType;
        [Tooltip("Only used if Goal Type is TimeLimit")]
        public float timeLimit;
    }

    public struct StarResult
    {
        public StarGoal goal;
        public bool achieved;
    }
}

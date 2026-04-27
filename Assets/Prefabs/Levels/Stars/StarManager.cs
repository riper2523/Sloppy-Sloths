using System.Collections.Generic;
using UnityEngine;

public class StarManager : MonoBehaviour
{   
    private LevelData currentLevel;
    private List<int> collectedStarIDs = new List<int>();
    private float levelTimer = 0f;
    private bool isDriving = false;

    private void OnEnable()
    {
        GameEvents.OnStarCollected += HandleStarCollected;
    }

    private void OnDisable()
    {
        GameEvents.OnStarCollected -= HandleStarCollected;
    }

    public void InitializeLevel(LevelData data)
    {
        currentLevel = data;
        collectedStarIDs.Clear();
        levelTimer = 0f;
        isDriving = false;
    }

    public void ReloadLevel()
    {
        collectedStarIDs.Clear();
        levelTimer = 0f;
        isDriving = false;
    }

    public void StartDrivingTimer()
    {
        isDriving = true;
    }

    private void Update()
    {
        if (isDriving)
        {
            levelTimer += Time.deltaTime;
        }
    }

    private void HandleStarCollected(int starID)
    {
        collectedStarIDs.Add(starID);
    }

    public bool[] EvaluateStars()
    {
        bool[] earnedStars = new bool[currentLevel.starGoals.Length];

        for (int i = 0; i < currentLevel.starGoals.Length; i++)
        {
            StarGoal goal = currentLevel.starGoals[i];

            switch (goal.goalType)
            {
                case StarGoalType.FinishLevel:
                    earnedStars[i] = true;
                    break;
                case StarGoalType.CollectStar:
                    earnedStars[i] = collectedStarIDs.Contains(i);
                    break;
                case StarGoalType.TimeLimit:
                    earnedStars[i] = levelTimer <= goal.timeLimit;
                    break;
            }
        }
        return earnedStars;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class StarManager : MonoBehaviour
{   
    [Header("Listening To")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;
    [SerializeField] private VoidEventChannelSO playLevelEvent;
    [SerializeField] private VoidEventChannelSO finishLineCrossedEvent;
    [SerializeField] private IntEventChannelSO starCollectedEvent;
    [Header("Broadcasting To")]
    [SerializeField] private LevelResultEventChannelSO levelCompletedEvent;

    private LevelData currentLevel;
    private List<int> collectedStarIDs = new List<int>();
    private float levelTimer = 0f;
    private bool isDriving = false;
    private int starsCollected = 0;

    private void OnEnable()
    {
        loadLevelEvent.OnEventRaised += InitializeLevel;
        playLevelEvent.OnEventRaised += StartDrivingTimer;
        finishLineCrossedEvent.OnEventRaised += HandleLevelFinished;
        starCollectedEvent.OnEventRaised += HandleStarCollected;
    }

    private void OnDisable()
    {
        loadLevelEvent.OnEventRaised -= InitializeLevel;
        playLevelEvent.OnEventRaised -= StartDrivingTimer;
        finishLineCrossedEvent.OnEventRaised -= HandleLevelFinished;
        starCollectedEvent.OnEventRaised -= HandleStarCollected;
    }

    private void InitializeLevel(LevelData data)
    {
        currentLevel = data;
        collectedStarIDs.Clear();
        levelTimer = 0f;
        isDriving = false;
        starsCollected = 0;
    }

    private void StartDrivingTimer() => isDriving = true;
    private void HandleStarCollected(int starID) 
    {
        Debug.Log($"Star Collected: {starID}");
        collectedStarIDs.Add(starID);
        starsCollected++;
    }

    private void Update()
    {
        if (isDriving) 
        {
            levelTimer += Time.deltaTime;
        }
    }

    private void HandleLevelFinished()
    {
        isDriving = false;
        
        LevelResult result = new LevelResult
        {
            completionTime = levelTimer,
            starResults = new List<StarResult>()
        };

        for (int i = 0; i < currentLevel.starGoals.Count; i++)
        {
            StarGoal goal = currentLevel.starGoals[i];
            bool earned = false;

            switch (goal.goalType)
            {
                case StarGoalType.FinishLevel: earned = true; break;
                case StarGoalType.CollectStar: earned = collectedStarIDs.Contains(i); break;
                case StarGoalType.TimeLimit: earned = levelTimer <= goal.timeLimit; break;
                case StarGoalType.NoDamage: earned = true; break; // Placeholder
            }

            result.starResults.Add(new StarResult { goalType = goal.goalType, achieved = earned });
        }

        levelCompletedEvent.RaiseEvent(result);
    }
}

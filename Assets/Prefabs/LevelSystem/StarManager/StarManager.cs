using System.Collections.Generic;
using UnityEngine;
using Assets.Prefabs.LevelSystem.StarManager;

public class StarManager : MonoBehaviour
{   
    [Header("Listening To")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;
    [SerializeField] private VoidEventChannelSO restartLevelEvent;
    [SerializeField] private VoidEventChannelSO playLevelEvent;
    [SerializeField] private VoidEventChannelSO finishLineCrossedEvent;
    [SerializeField] private CollectibleStarEventChannelSO starSpawnedEvent;
    [SerializeField] private CollectibleStarEventChannelSO starCollectedEvent; 

    [Header("Broadcasting To")]
    [SerializeField] private LevelResultEventChannelSO levelCompletedEvent;

    private LevelData currentLevel;
    private HashSet<int> sessionCollectedStarIDs = new HashSet<int>();
    private float levelTimer = 0f;
    private bool isDriving = false;

    private void OnEnable()
    {
        loadLevelEvent.OnEventRaised += InitializeLevelData;
        restartLevelEvent.OnEventRaised += HandleRestart;
        playLevelEvent.OnEventRaised += StartDrivingTimer;
        finishLineCrossedEvent.OnEventRaised += HandleLevelFinished;
        starCollectedEvent.OnEventRaised += HandleStarCollected;
        starSpawnedEvent.OnEventRaised += HandleStarSpawned;
    }

    private void OnDisable()
    {
        loadLevelEvent.OnEventRaised -= InitializeLevelData;
        restartLevelEvent.OnEventRaised -= HandleRestart;
        playLevelEvent.OnEventRaised -= StartDrivingTimer;
        finishLineCrossedEvent.OnEventRaised -= HandleLevelFinished;
        starCollectedEvent.OnEventRaised -= HandleStarCollected;
        starSpawnedEvent.OnEventRaised -= HandleStarSpawned;
    }

    private void InitializeLevelData(LevelData data)
    {
        currentLevel = data;
        sessionCollectedStarIDs.Clear();
        levelTimer = 0f;
        isDriving = false;
    }

    private void HandleRestart()
    {
        sessionCollectedStarIDs.Clear();
        levelTimer = 0f;
        isDriving = false;
    }

    private void StartDrivingTimer() => isDriving = true;

    private void HandleStarSpawned(CollectibleStar star)
    {
        // can switch star color here based on being collected
    }
    
    private void HandleStarCollected(CollectibleStar star) 
    {
        if (!isDriving) return;
        sessionCollectedStarIDs.Add(star.starID);
        star.SetCollected();
    }

    private void Update()
    {
        if (isDriving) levelTimer += Time.deltaTime;
    }

    private void HandleLevelFinished()
    {
        isDriving = false;
        
        LevelResult result = new LevelResult
        {
            completionTime = levelTimer,
            starResults = new List<StarResult>(),
            collectedStarIDs = new List<int>(sessionCollectedStarIDs)
        };

        int collectibleStarIndex = 0;

        for (int i = 0; i < currentLevel.starGoals.Count; i++)
        {
            StarGoal goal = currentLevel.starGoals[i];
            bool earned = false;

            switch (goal.goalType)
            {
                case StarGoalType.FinishLevel: earned = true; break;
                case StarGoalType.CollectStar: 
                    earned = sessionCollectedStarIDs.Contains(collectibleStarIndex);
                    collectibleStarIndex++; 
                    break;
                case StarGoalType.TimeLimit: earned = levelTimer <= goal.timeLimit; break;
            }

            result.starResults.Add(new StarResult { goal = goal, achieved = earned });
        }

        levelCompletedEvent.RaiseEvent(result);
    }
}

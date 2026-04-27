using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private MapLoader mapLoader;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private StarManager starManager;
    [SerializeField] private PanelManager panelManager;
    [SerializeField] private InventoryManager inventoryManager;
    [Header("Level Data")]
    [SerializeField] private LevelData levelData;

    private void Start()
    {
        StartLevel(levelData);
    }

    private void OnEnable()
    {
        GameEvents.OnBuild += StartPlayPhase;
        GameEvents.OnFinishLineCrossed += HandleLevelWon;
        GameEvents.OnRestartLevel += RestartLevel;
    }

    private void OnDisable()
    {
        GameEvents.OnBuild -= StartPlayPhase;
        GameEvents.OnFinishLineCrossed -= HandleLevelWon;
        GameEvents.OnRestartLevel -= RestartLevel;
    }

    private void StartLevel(LevelData data)
    {
        mapLoader.LoadMap(data);
        starManager.InitializeLevel(data);
        inventoryManager.InitializeLevel(data.startingItems);
        gridManager.InitializeLevel(data);
        panelManager.ShowBuildPanel();
    }

    private void StartPlayPhase()
    {
        gridManager.Build();
        starManager.StartDrivingTimer();
        panelManager.ShowGamePanel();
    }

    private void HandleLevelWon()
    {
        bool[] earnedStars = starManager.EvaluateStars();

        panelManager.ShowWinPanel(earnedStars);
    }

    private void RestartLevel()
    {
        mapLoader.LoadMap(levelData);
        gridManager.Restart();
        starManager.ReloadLevel();
        panelManager.ShowBuildPanel();
    }
}

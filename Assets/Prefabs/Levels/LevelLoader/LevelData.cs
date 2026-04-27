using UnityEngine;
[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Environment")]
    public GameObject mapPrefab;
    [Header("Grid Settings")]
    public int gridSizeX = 5;
    public int gridSizeY = 5;
    public int positionX = 0;
    public int positionY = 0;
    [Header("Starting Inventory")]
    public Inventory startingItems;

    [Header("Star Goals")]
    public StarGoal[] starGoals;
}

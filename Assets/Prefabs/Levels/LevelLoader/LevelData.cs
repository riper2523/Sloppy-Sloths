using UnityEngine;
using System.Collections.Generic;[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
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
    public List<InventoryEntry> startingItems;

    [Header("Finish Line Settings")]
    public Vector3 finishLinePosition;
    public Vector3 finishLineScale = Vector3.one;
}

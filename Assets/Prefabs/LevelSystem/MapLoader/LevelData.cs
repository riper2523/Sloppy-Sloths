using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Map")]
    public GameObject mapPrefab;
    [Header("Starting Inventory")]
    public Inventory startingItems;

    [Header("Star Goals")]
    public List<StarGoal> starGoals;
}

using System.Collections.Generic;
using UnityEngine;
using Assets.Prefabs.LevelSystem.StarManager;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Save Data")]
    public string uniqueID;

    [Header("Map")]
    public GameObject mapPrefab;
    
    [Header("Starting Inventory")]
    public Inventory startingItems;

    [Header("Star Goals")]
    public List<StarGoal> starGoals;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Assets.Prefabs.LevelSystem.StarManager;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    #region Constants
    private const string PROP_STAR_GOALS = "starGoals";
    private const string PROP_GOAL_TYPE = "goalType";
    private const string PROP_MAP_PREFAB = "mapPrefab";

    private const string MSG_NO_MAP = "Assign a Map Prefab to validate star counts.";
    private const string MSG_MATCH_SUCCESS = "✓ PERFECT MATCH: {0} physical stars perfectly baked and match {1} goals.";
    private const string MSG_COUNT_MISMATCH = "⚠️ MISMATCH ERROR!\nPrefab has: {0} stars.\nData asks for: {1} 'Collect Star' goals.";
    private const string MSG_ID_MISMATCH = "⚠️ ID MISMATCH!\nThe physical stars have incorrect or out-of-order IDs.";
    
    private const string BTN_AUTO_FIX = "Auto-Fix: Match Data to Prefab & Bake IDs";
    private const string BTN_REBAKE = "Auto-Fix: Re-Bake Star IDs";
    
    private const int TYPE_COLLECT_STAR = (int)StarGoalType.CollectStar;
    #endregion

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        LevelData data = (LevelData)target;
        EditorGUILayout.Space(10);

        if (data.mapPrefab == null)
        {
            EditorGUILayout.HelpBox(MSG_NO_MAP, MessageType.Warning);
        }
        else
        {
            PerformValidation(data);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void PerformValidation(LevelData data)
    {
        // 1. Gather Data
        CollectibleStar[] physicalStars = data.mapPrefab.GetComponentsInChildren<CollectibleStar>(true);
        SerializedProperty starGoalsProp = serializedObject.FindProperty(PROP_STAR_GOALS);
        int requiredStarsInData = CountCollectStarGoals(starGoalsProp);

        // 2. Logic Checks
        bool countMismatch = physicalStars.Length != requiredStarsInData;
        bool idMismatch = !countMismatch && HasIdMismatch(physicalStars);

        // 3. Draw UI
        if (countMismatch)
        {
            DrawCountMismatchUI(data, starGoalsProp, physicalStars.Length, requiredStarsInData);
        }
        else if (idMismatch)
        {
            DrawIdMismatchUI(data.mapPrefab);
        }
        else if (requiredStarsInData > 0)
        {
            EditorGUILayout.HelpBox(string.Format(MSG_MATCH_SUCCESS, physicalStars.Length, requiredStarsInData), MessageType.Info);
        }
    }

    #region UI Drawers
    private void DrawCountMismatchUI(LevelData data, SerializedProperty starGoalsProp, int prefabCount, int dataCount)
    {
        EditorGUILayout.HelpBox(string.Format(MSG_COUNT_MISMATCH, prefabCount, dataCount), MessageType.Error);

        if (GUILayout.Button(BTN_AUTO_FIX))
        {
            SyncGoalCountToPrefab(starGoalsProp, prefabCount - dataCount);
            BakeStarIDs(data.mapPrefab);
        }
    }

    private void DrawIdMismatchUI(GameObject mapPrefab)
    {
        EditorGUILayout.HelpBox(MSG_ID_MISMATCH, MessageType.Warning);

        if (GUILayout.Button(BTN_REBAKE))
        {
            BakeStarIDs(mapPrefab);
            Debug.Log("Star IDs successfully re-baked!");
        }
    }
    #endregion

    #region Logic Helpers
    private int CountCollectStarGoals(SerializedProperty list)
    {
        int count = 0;
        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).FindPropertyRelative(PROP_GOAL_TYPE).enumValueIndex == TYPE_COLLECT_STAR)
                count++;
        }
        return count;
    }

    private bool HasIdMismatch(CollectibleStar[] stars)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i].starID != i) return true;
        }
        return false;
    }

    private void SyncGoalCountToPrefab(SerializedProperty starGoalsProp, int difference)
    {
        if (difference > 0)
        {
            AddGoals(starGoalsProp, difference);
        }
        else
        {
            RemoveGoals(starGoalsProp, Mathf.Abs(difference));
        }
    }

    private void AddGoals(SerializedProperty list, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int newIndex = list.arraySize;
            list.InsertArrayElementAtIndex(newIndex);
            list.GetArrayElementAtIndex(newIndex).FindPropertyRelative(PROP_GOAL_TYPE).enumValueIndex = TYPE_COLLECT_STAR;
        }
    }

    private void RemoveGoals(SerializedProperty list, int count)
    {
        int removedSoFar = 0;
        for (int i = list.arraySize - 1; i >= 0 && removedSoFar < count; i--)
        {
            if (list.GetArrayElementAtIndex(i).FindPropertyRelative(PROP_GOAL_TYPE).enumValueIndex == TYPE_COLLECT_STAR)
            {
                list.DeleteArrayElementAtIndex(i);
                removedSoFar++;
            }
        }
    }

    private void BakeStarIDs(GameObject mapPrefab)
    {
        string assetPath = AssetDatabase.GetAssetPath(mapPrefab);
        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
        {
            var stars = editingScope.prefabContentsRoot.GetComponentsInChildren<CollectibleStar>(true);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].starID = i;
                EditorUtility.SetDirty(stars[i]);
            }
        }
    }
    #endregion
}
#endif

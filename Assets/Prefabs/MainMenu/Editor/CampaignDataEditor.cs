#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(CampaignData))]
public class CampaignDataEditor : Editor
{
    // Change this to wherever your chapters are stored!
    private const string LEVELS_ROOT_PATH = "Assets/Prefabs/Levels"; 
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CampaignData campaign = (CampaignData)target;

        EditorGUILayout.Space(15);
        if (GUILayout.Button("Auto-Sync Campaign from Folders", GUILayout.Height(30)))
        {
            SyncCampaign(campaign);
        }
    }

    private void SyncCampaign(CampaignData campaign)
    {
        // 1. Ensure the root folder exists
        if (!AssetDatabase.IsValidFolder(LEVELS_ROOT_PATH))
        {
            Debug.LogError($"Cannot find folder: {LEVELS_ROOT_PATH}");
            return;
        }

        Undo.RecordObject(campaign, "Sync Campaign Data");
        campaign.chapters = new List<ChapterData>();

        // 2. Get all subfolders (Chapters) inside the root folder
        string[] chapterFolders = AssetDatabase.GetSubFolders(LEVELS_ROOT_PATH);
        
        // Sort folders alphabetically
        System.Array.Sort(chapterFolders);

        foreach (string folderPath in chapterFolders)
        {
            // Extract the name of the folder (e.g., "Chapter_01")
            string chapterName = Path.GetFileName(folderPath);

            // 3. Find or Create the ChapterData SO
            ChapterData chapterData = GetOrCreateChapterData(folderPath, chapterName);

            // 4. Find all LevelData assets inside this chapter folder
            Undo.RecordObject(chapterData, "Sync Chapter Data");
            chapterData.levels = new List<LevelData>();

            string[] levelGuids = AssetDatabase.FindAssets("t:LevelData", new[] { folderPath });
            
            // Convert GUIDs to actual LevelData objects
            List<LevelData> foundLevels = new List<LevelData>();
            foreach (string guid in levelGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (level != null) foundLevels.Add(level);
            }

            // 5. Sort levels alphabetically by file name and add to chapter
            foundLevels.Sort((a, b) => a.name.CompareTo(b.name));
            chapterData.levels.AddRange(foundLevels);

            // 6. Add the completed chapter to the Campaign
            campaign.chapters.Add(chapterData);

            EditorUtility.SetDirty(chapterData);
        }

        EditorUtility.SetDirty(campaign);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green><b>Campaign Synced!</b></color> Found {campaign.chapters.Count} Chapters.");
    }

    private ChapterData GetOrCreateChapterData(string folderPath, string chapterName)
    {
        // We will look for a file named "ChapterData_Name.asset" inside the chapter folder
        string expectedAssetPath = $"{folderPath}/ChapterData_{chapterName}.asset";

        ChapterData existingData = AssetDatabase.LoadAssetAtPath<ChapterData>(expectedAssetPath);

        if (existingData != null)
        {
            return existingData;
        }

        // If it doesn't exist, we generate it automatically!
        ChapterData newData = ScriptableObject.CreateInstance<ChapterData>();
        newData.chapterName = chapterName; // Default the title to the folder name
        
        AssetDatabase.CreateAsset(newData, expectedAssetPath);
        return newData;
    }
}
#endif

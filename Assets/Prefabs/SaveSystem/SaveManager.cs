using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Assets.Prefabs.LevelSystem.StarManager;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Event Channels")]
    public VoidEventChannelSO playLevelEvent;
    public LevelResultEventChannelSO levelCompletedEvent;
    public CurrentSessionSO currentSessionSO;

    [Header("Game Data")]
    public GameSaveData CurrentSave;

    [Header("Save Settings")]
    [SerializeField]
    private string profileId = "Slot_1";

    private Dictionary<string, LevelSaveData> levelCache = new Dictionary<string, LevelSaveData>();

    private string ProfileMetaFilePath => PathCombine(Application.persistentDataPath, "Saves", profileId, "meta.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    private void OnEnable()
    {
        playLevelEvent.OnEventRaised += HandlePlayLevel;
        levelCompletedEvent.OnEventRaised += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        playLevelEvent.OnEventRaised -= HandlePlayLevel;
        levelCompletedEvent.OnEventRaised -= HandleLevelCompleted;
    }

    public void LoadData()
    {
        try
        {
            if (File.Exists(ProfileMetaFilePath))
            {
                string json = File.ReadAllText(ProfileMetaFilePath);
                CurrentSave = JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();
            }
            else
            {
                CurrentSave = new GameSaveData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load profile meta save. error: {e.Message}");
            CurrentSave = new GameSaveData();
        }
        
        levelCache.Clear();
    }

    public void SaveData()
    {
        try
        {
            CurrentSave.lastPlayedUnixTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string json = JsonUtility.ToJson(CurrentSave, true);
            WriteToFileAtomic(ProfileMetaFilePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save profile meta. error: {e.Message}");
        }
    }

    public LevelSaveData GetLevelData(string levelID)
    {
        if (levelCache.TryGetValue(levelID, out var cachedLevel))
        {
            return cachedLevel;
        }

        var loadedLevel = LoadLevelFromDisk(levelID);
        if (loadedLevel == null)
        {
            loadedLevel = new LevelSaveData { levelID = levelID };
        }
        
        levelCache[levelID] = loadedLevel;
        return loadedLevel;
    }

    public bool TryGetLevelData(string levelID, out LevelSaveData levelSave)
    {
        levelSave = GetLevelData(levelID);
        return levelSave != null;
    }

    public bool HasCollectedStar(string levelID, int starID)
    {
        return TryGetLevelData(levelID, out LevelSaveData levelSave)
            && levelSave.collectedStarIDs.Contains(starID);
    }

    public int GetEarnedStarCount(LevelData levelData)
    {
        if (TryGetLevelData(levelData.uniqueID, out LevelSaveData levelSave) && levelSave.achievedGoalIndices != null && levelSave.achievedGoalIndices.Count > 0)
        {
            return levelSave.achievedGoalIndices.Count;
        }

        int earnedCount = 0;
        int collectibleStarIndex = 0;

        foreach (StarGoal goal in levelData.starGoals)
        {
            switch (goal.goalType)
            {
                case StarGoalType.FinishLevel:
                    if (levelSave.bestTime >= 0f)
                    {
                        earnedCount++;
                    }
                    break;
                case StarGoalType.CollectStar:
                    if (levelSave.collectedStarIDs.Contains(collectibleStarIndex))
                    {
                        earnedCount++;
                    }
                    collectibleStarIndex++;
                    break;
                case StarGoalType.TimeLimit:
                    if (levelSave.bestTime >= 0f && levelSave.bestTime <= goal.timeLimit)
                    {
                        earnedCount++;
                    }
                    break;
            }
        }

        return earnedCount;
    }

    private void HandlePlayLevel()
    {
        string levelID = currentSessionSO.activeLevel.uniqueID;
        LevelSaveData levelSave = GetLevelData(levelID);

        GridManager gridManager = FindAnyObjectByType<GridManager>();
        levelSave.lastAttemptedGrid = gridManager.ExportGridState();
        SaveLevelToDisk(levelSave);
    }

    private void HandleLevelCompleted(LevelResult result)
    {
        string levelID = currentSessionSO.activeLevel.uniqueID;
        LevelSaveData levelSave = GetLevelData(levelID);

        if (levelSave.bestTime < 0 || result.completionTime < levelSave.bestTime)
        {
            levelSave.bestTime = result.completionTime;
        }

        if (result.collectedStarIDs != null)
        {
            foreach (int starID in result.collectedStarIDs)
            {
                if (!levelSave.collectedStarIDs.Contains(starID))
                {
                    levelSave.collectedStarIDs.Add(starID);
                }
            }
        }

        if (result.starResults != null)
        {
            for (int i = 0; i < result.starResults.Count; i++)
            {
                if (result.starResults[i].achieved && !levelSave.achievedGoalIndices.Contains(i))
                {
                    levelSave.achievedGoalIndices.Add(i);
                }
            }
        }

        SaveLevelToDisk(levelSave);
        SaveData();
    }

    private string GetProfileDirectory(string profileId)
    {
        return PathCombine(Application.persistentDataPath, "Saves", profileId);
    }

    private string GetLevelsDirectory(string profileId)
    {
        return PathCombine(GetProfileDirectory(profileId), "Levels");
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    private void WriteToFileAtomic(string filePath, string content)
    {
        try
        {
            string dir = Path.GetDirectoryName(filePath);
            EnsureDirectoryExists(dir);

            string tmp = filePath + ".tmp";
            string bak = filePath + ".bak";
            
            File.WriteAllText(tmp, content);
            
            if (File.Exists(filePath))
            {
                if (File.Exists(bak)) File.Delete(bak);
                File.Move(filePath, bak);
            }
            File.Move(tmp, filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Atomic write failed for {filePath}: {e.Message}");
        }
    }

    private void SaveLevelToDisk(LevelSaveData levelData)
    {
        if (levelData == null) return;

        string levelsDir = GetLevelsDirectory(profileId);
        EnsureDirectoryExists(levelsDir);
        string filePath = PathCombine(levelsDir, levelData.levelID + ".json");

        string json = JsonUtility.ToJson(levelData, true);
        WriteToFileAtomic(filePath, json);
    }

    private LevelSaveData LoadLevelFromDisk(string levelId)
    {
        string levelsDir = GetLevelsDirectory(profileId);
        string filePath = PathCombine(levelsDir, levelId + ".json");
        if (!File.Exists(filePath)) return null;
        try
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<LevelSaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to read level save {filePath}: {e.Message}");
            return null;
        }
    }

    private string PathCombine(params string[] parts)
    {
        return System.IO.Path.Combine(parts);
    }
}

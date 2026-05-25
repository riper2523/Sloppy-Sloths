using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampaignUIBuilder : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private CampaignData campaignData;
    [SerializeField] private CurrentSessionSO currentSession;
    [SerializeField] private string levelSceneName = "LevelScene";
    
    [Header("Controllers")]
    [SerializeField] private MainMenuController menuController;
    
    [Header("Chapter Panel Elements")]
    [SerializeField] private Transform chapterContainer;
    [SerializeField] private GameObject chapterButtonPrefab;
    
    [Header("Level Panel Elements")]
    [SerializeField] private Transform levelContainer;
    [SerializeField] private GameObject levelButtonPrefab;

    private void Start()
    {
        PopulateChapters();
        
        if (currentSession != null && currentSession.returnToLevelSelection && currentSession.activeChapter != null)
        {
            currentSession.returnToLevelSelection = false; // Reset for next time
            OnChapterClicked(currentSession.activeChapter);
        }
    }

    private void PopulateChapters()
    {
        if (campaignData == null || campaignData.chapters == null)
        {
            Debug.LogWarning("CampaignData is not assigned or chapters list is null in CampaignUIBuilder.");
            return;
        }

        // Clear existing (in case of re-population)
        foreach (Transform child in chapterContainer) Destroy(child.gameObject);

        foreach (var chapter in campaignData.chapters)
        {
            var btnGo = Instantiate(chapterButtonPrefab, chapterContainer);
            if (btnGo.TryGetComponent<ChapterButtonUI>(out var chapterUI))
            {
                chapterUI.Setup(chapter, OnChapterClicked);
            }
        }
    }

    private void OnChapterClicked(ChapterData chapter)
    {
        if (currentSession != null)
        {
            currentSession.activeChapter = chapter;
        }

        PopulateLevels(chapter);
        if (menuController != null)
        {
            menuController.ShowLevelSelectionPanel();
        }
    }

    private void PopulateLevels(ChapterData chapter)
    {
        // Clear existing levels from another chapter
        foreach (Transform child in levelContainer) Destroy(child.gameObject);

        for (int i = 0; i < chapter.levels.Count; i++)
        {
            var level = chapter.levels[i];
            if (level == null) continue;

            var btnGo = Instantiate(levelButtonPrefab, levelContainer);
            if (btnGo.TryGetComponent<LevelButtonUI>(out var levelUI))
            {
                levelUI.Setup(level, i + 1, OnLevelClicked);
            }
        }
    }

    private void OnLevelClicked(LevelData level)
    {
        if (currentSession != null)
        {
            currentSession.activeLevel = level;
            currentSession.returnToLevelSelection = true;
            SceneManager.LoadScene(levelSceneName);
        }
        else
        {
            Debug.LogError("No CurrentSessionSO assigned. Cannot start level.");
        }
    }
}

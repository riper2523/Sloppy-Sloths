using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using Assets.Prefabs.LevelSystem.StarManager;

public class WinPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText; 

    public void DisplayResults(LevelResult result)
    {
        LevelSaveData levelSave = null;
        if (SaveManager.Instance != null && SaveManager.Instance.currentSessionSO != null && SaveManager.Instance.currentSessionSO.activeLevel != null)
        {
            string levelID = SaveManager.Instance.currentSessionSO.activeLevel.uniqueID;
            levelSave = SaveManager.Instance.GetLevelData(levelID);
        }

        bool supportsUnicodeStars = resultText != null && resultText.font != null && resultText.font.HasCharacter('★');
        string filledStar = supportsUnicodeStars ? "★" : "[X]";
        string emptyStar = supportsUnicodeStars ? "☆" : "[ ]";

        int totalAchievedCount = 0;
        string starsVisual = "";
        string details = "";

        for (int i = 0; i < result.starResults.Count; i++)
        {
            StarResult starResult = result.starResults[i];
            
            // It's achieved if they got it in this run OR they already had it from a previous run
            bool isAchieved = starResult.achieved || (levelSave != null && levelSave.achievedGoalIndices.Contains(i));

            if (isAchieved)
            {
                totalAchievedCount++;
                starsVisual += filledStar + " ";
            }
            else
            {
                starsVisual += emptyStar + " ";
            }

            details += (isAchieved ? filledStar : emptyStar) + " " + starResult.goal.goalType + "\n";
        }

        resultText.text =
            $"Level Completed!\n" +
            $"Time: {result.completionTime:F2} seconds\n" +
            $"Stars: {starsVisual.TrimEnd()}\n" +
            $"Achieved: {totalAchievedCount}/{result.starResults.Count}\n\n" +
            $"Star Details:\n{details}";
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class LevelButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;

    private Button button;
    private LevelData levelData;
    public void Setup(LevelData data, int levelNumber, UnityAction<LevelData> onClick)
    {
        levelData = data;
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(levelData));

        int starsCount = SaveManager.Instance.GetEarnedStarCount(data);

        if (titleText != null) 
        {
            titleText.text = $"{levelNumber}\n<size=50%>{starsCount} / {data.starGoals.Count}</size>";
        }
    }
}

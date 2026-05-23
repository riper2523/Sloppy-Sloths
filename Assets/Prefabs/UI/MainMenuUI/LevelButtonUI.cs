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

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(LevelData data, int levelNumber, UnityAction<LevelData> onClick)
    {
        levelData = data;
        if (titleText != null) titleText.text = levelNumber.ToString();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(levelData));
    }
}

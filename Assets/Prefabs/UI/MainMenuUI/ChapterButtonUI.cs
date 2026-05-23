using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class ChapterButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image iconImage;

    private Button button;
    private ChapterData chapterData;
    public void Setup(ChapterData data, UnityAction<ChapterData> onClick)
    {
        chapterData = data;
        if (titleText != null) titleText.text = data.chapterName;
        if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;

        if (button == null)
        {
            button = GetComponent<Button>();
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(chapterData));
    }
}

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
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(ChapterData data, UnityAction<ChapterData> onClick)
    {
        chapterData = data;
        if (titleText != null) titleText.text = data.chapterName;
        if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(chapterData));
    }
}

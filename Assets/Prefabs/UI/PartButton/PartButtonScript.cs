using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartButtonScript : MonoBehaviour
{
    [SerializeField]
    private PartData partData;
    [SerializeField]
    private Image buttonImage;
    [SerializeField]
    private Button button;
    [SerializeField]
    private TMP_Text buttonText;
    [SerializeField]
    private GridManager gridManager;
    private int partCount = 1;
    void setPartCount(int count)
    {
        partCount = count;
        buttonText.text = partCount.ToString();
    }
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            gridManager.SelectPart(partData);
        });
        buttonImage.sprite = partData.partSpriteUI;
        buttonText.text = partCount.ToString();
    }
}

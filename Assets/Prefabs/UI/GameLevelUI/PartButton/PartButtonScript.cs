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
    private int partCount = 0;
    void Start()
    {
        if (gridManager != null && partData != null)
        {
            Setup();
        }

    }
    public void SetPartCount(int count)
    {
        partCount = count;
        buttonText.text = partCount.ToString();
    }
    public void Initialize(PartData data, GridManager manager)
    {
        partData = data;
        gridManager = manager;
        Setup();
    }
    private void Setup()
    {
        button.onClick.AddListener(() =>
                   {
                       gridManager.SelectPart(partData);
                   });
        buttonImage.sprite = partData.partSpriteUI;
        buttonText.text = partCount.ToString();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PartButtonScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
    private GameObject ghostIcon;
    private Camera mainCam;
    void Start()
    {
        mainCam = Camera.main;

        if (gridManager != null && partData != null)
        {
            Setup();
        }
    }

    public void SetPartCount(int count)
    {
        partCount = count;
        buttonText.text = partCount.ToString();
        button.interactable = (partCount > 0);
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
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (partCount <= 0) return;

        gridManager.SelectPart(partData);

        ghostIcon = new GameObject("UI_DragGhost");
        SpriteRenderer sr = ghostIcon.AddComponent<SpriteRenderer>();
        sr.sprite = buttonImage.sprite;
        sr.sortingOrder = 999;

        Vector3 pos = mainCam.ScreenToWorldPoint(eventData.position);
        pos.z = 0;
        ghostIcon.transform.position = pos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            Vector3 pos = mainCam.ScreenToWorldPoint(eventData.position);
            pos.z = 0;
            ghostIcon.transform.position = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            Destroy(ghostIcon);
        }

        if (partCount <= 0) return;

        Vector3 worldPos = mainCam.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;

        gridManager.DropPartAtWorldPosition(worldPos);
    }
}
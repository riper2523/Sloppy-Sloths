using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using System.Threading;
using System.Threading.Tasks;

public class TileMapClicker : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public UnityEvent<Vector3Int> onTileClicked;
    public UnityEvent<Vector3Int> onTileDoubleClicked;
    private Tilemap tilemap;
    private CancellationTokenSource cancelToken;
    [SerializeField] private int doubleClickThreshold = 250;
    private int clickCounter = 0;
    private float lastClickTime = 0f;
    [SerializeField] private GridManager gridManager;
    private Camera mainCam;
    private GameObject ghostIcon;
    private PartData draggedPart;
    private float lastDragEndTime = 0f;
    private bool isDragging = false;
    void Awake()
    {
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }
        mainCam = Camera.main;
    }
    public async void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging || Time.time - lastDragEndTime < 0.1f) return;
        Vector3 worldPosition = eventData.pointerCurrentRaycast.worldPosition;
        Vector3Int tilePosition = tilemap.WorldToCell(worldPosition);
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= (doubleClickThreshold / 1000f))
        {
            clickCounter++;
        }
        else
        {
            clickCounter = 1;
        }
        lastClickTime = Time.time;
        if (clickCounter == 1)
        {
            cancelToken = new CancellationTokenSource();
            try
            {
                await Task.Delay(doubleClickThreshold, cancelToken.Token);
                onTileClicked.Invoke(tilePosition);
            }
            catch (TaskCanceledException)
            {
            }
        }
        else if (clickCounter >= 2)
        {
            cancelToken?.Cancel();
            onTileDoubleClicked.Invoke(tilePosition);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        cancelToken?.Cancel();
        clickCounter = 0;

        Vector3 worldPosition = eventData.pointerCurrentRaycast.worldPosition;
        Vector3Int tilePosition = tilemap.WorldToCell(worldPosition);

        draggedPart = gridManager.GrabPartFromGrid(tilePosition);

        if (draggedPart != null)
        {
            ghostIcon = new GameObject("GridDragGhost");
            SpriteRenderer sr = ghostIcon.AddComponent<SpriteRenderer>();
            sr.sprite = draggedPart.partSpriteUI;
            sr.sortingOrder = 999;

            Vector3 pos = mainCam.ScreenToWorldPoint(eventData.position);
            pos.z = 0;
            ghostIcon.transform.position = pos;
        }
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
        isDragging = false;
        if (ghostIcon != null) Destroy(ghostIcon);

        if (draggedPart != null)
        {
            Vector3 worldPosition = mainCam.ScreenToWorldPoint(eventData.position);
            worldPosition.z = 0;
            gridManager.SelectPart(draggedPart);
            gridManager.DropPartAtWorldPosition(worldPosition);
            draggedPart = null;
        }
        lastDragEndTime = Time.time;

    }
}
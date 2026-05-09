using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class TileMapClicker : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent<Vector3Int> onTileClicked;
    public UnityEvent<Vector3Int> onTileDoubleClicked;
    private Tilemap tilemap;
    private CancellationTokenSource cancelToken;
    [SerializeField] private int doubleClickThreshold = 250;
    private int clickCounter = 0;
    private float lastClickTime = 0f;
    void Awake()
    {
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }
    }
    public async void OnPointerClick(PointerEventData eventData)
    {
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
}

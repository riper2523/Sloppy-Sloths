#nullable enable
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [SerializeField] private Transform environmentParent = null!;
    [Header("Listening To")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent = null!;
    [SerializeField] private VoidEventChannelSO restartLevelEvent = null!;
    [SerializeField] private GridManager gridManager = null!;

    private GameObject? currentMapInstance;
    private GameObject? mapPrefab;

    public static LevelData? PendingLevel { get; set; }

    private void Awake()
    {
        Debug.Assert(environmentParent != null, "MapLoader: environmentParent is not assigned!");
        Debug.Assert(loadLevelEvent != null, "MapLoader: loadLevelEvent is not assigned!");
        Debug.Assert(restartLevelEvent != null, "MapLoader: restartLevelEvent is not assigned!");
        Debug.Assert(gridManager != null, "MapLoader: gridManager is not assigned!");
    }

    private void OnEnable()
    {
        loadLevelEvent.OnEventRaised += HandleLoadEvent;
        restartLevelEvent.OnEventRaised += ReloadMap;
    }

    private void OnDisable() 
    {
        loadLevelEvent.OnEventRaised -= LoadMap;
        restartLevelEvent.OnEventRaised -= ReloadMap;
    }

    public void LoadMap(LevelData levelData)
    {
        ClearMap();
        mapPrefab = levelData.mapPrefab;
        Debug.Log("Instantiating the mapPrefab");
        currentMapInstance = Instantiate(mapPrefab, environmentParent);
        currentMapInstance.SetActive(true);
        GridAnchor anchor = currentMapInstance.GetComponentInChildren<GridAnchor>();
        gridManager.InitializeLevel(anchor);
    }

    public void ReloadMap()
    {
        ClearMap();
        currentMapInstance = Instantiate(mapPrefab, environmentParent);
        Debug.Assert(currentMapInstance is not null);
        currentMapInstance!.SetActive(true);
    }

    private void ClearMap()
    {
        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
        }
    }
}

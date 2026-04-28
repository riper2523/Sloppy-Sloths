using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [SerializeField] private Transform environmentParent;
    [Header("Listening To")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;
    [SerializeField] private VoidEventChannelSO restartLevelEvent;
    [Header("Broadcasting On")]
    [SerializeField] private GridAnchorEventChannelSO anchorFoundEvent;
    
    private GameObject currentMapInstance;
    private GameObject mapPrefab;

    private void OnEnable() 
    {
        loadLevelEvent.OnEventRaised += LoadMap;
        restartLevelEvent.OnEventRaised += ReloadMap;
    }

    private void OnDisable() 
    {
        loadLevelEvent.OnEventRaised -= LoadMap;
        restartLevelEvent.OnEventRaised -= ReloadMap;
    }

    public void LoadMap(LevelData levelData)
    {
        mapPrefab = levelData.mapPrefab;
        currentMapInstance = Instantiate(mapPrefab, environmentParent);
        GridAnchor anchor = currentMapInstance.GetComponentInChildren<GridAnchor>();
        anchorFoundEvent.RaiseEvent(anchor);
    }

    public void ReloadMap()
    {
        ClearMap();
        currentMapInstance = Instantiate(mapPrefab, environmentParent);
        // GridAnchor anchor = currentMapInstance.GetComponentInChildren<GridAnchor>();
        // anchorFoundEvent.RaiseEvent(anchor);
    }

    private void ClearMap()
    {
        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
        }
    }
}

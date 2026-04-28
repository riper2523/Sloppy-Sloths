using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [SerializeField] private Transform environmentParent;
    [Header("Listening To")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;
    [Header("Broadcasting On")]
    [SerializeField] private GridAnchorEventChannelSO anchorFoundEvent;
    
    private GameObject currentMapInstance;

    private void OnEnable() => loadLevelEvent.OnEventRaised += LoadMap;
    private void OnDisable() => loadLevelEvent.OnEventRaised -= LoadMap;

    public void LoadMap(LevelData levelData)
    {
        ClearMap();

        currentMapInstance = Instantiate(levelData.mapPrefab, environmentParent);

        GridAnchor anchor = currentMapInstance.GetComponentInChildren<GridAnchor>();
        anchorFoundEvent.RaiseEvent(anchor);
    }

    private void ClearMap()
    {
        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
        }
    }
}

using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [SerializeField] private Transform environmentParent;
    [Header("Listening To")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;
    
    private GameObject currentMapInstance;

    private void OnEnable() => loadLevelEvent.OnEventRaised += LoadMap;
    private void OnDisable() => loadLevelEvent.OnEventRaised -= LoadMap;

    public void LoadMap(LevelData levelData)
    {
        ClearMap();

        currentMapInstance = Instantiate(levelData.mapPrefab, environmentParent);
    }

    private void ClearMap()
    {
        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
        }
    }
}

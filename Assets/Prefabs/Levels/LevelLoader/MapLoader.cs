using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [SerializeField] private Transform environmentParent;

    private GameObject currentMapInstance;

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

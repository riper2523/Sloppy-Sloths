using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private LevelData currentLevelToLoad;
    [SerializeField] private Transform environmentParent;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PanelManager panelManager;
    [SerializeField] private Transform finishLine;

    private GameObject currentMapInstance;

    private void Start()
    {
        if (currentLevelToLoad != null)
        {
            LoadLevel(currentLevelToLoad);
        }
    }

    public void LoadLevel(LevelData levelData)
    {
        currentLevelToLoad = levelData;

        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
        }

        currentMapInstance = Instantiate(levelData.mapPrefab, environmentParent);
        currentMapInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        if (finishLine != null)
        {
            finishLine.position = levelData.finishLinePosition;
            finishLine.localScale = levelData.finishLineScale;
            finishLine.gameObject.SetActive(true);
        }

        inventoryManager.InitializeLevel(levelData.startingItems);
        gridManager.InitializeLevel(levelData);

        if (panelManager != null)
        {
            panelManager.ShowBuildPanel();
        }
    }

    public void ReloadCurrentLevel()
    {
        if (currentLevelToLoad != null)
        {
            LoadLevel(currentLevelToLoad);
            gridManager.Restart();
        }
    }
}

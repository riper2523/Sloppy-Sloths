using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private LevelData currentLevelToLoad;
    [SerializeField] private Transform environmentParent;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PanelManager panelManager;
    [SerializeField] private StarManager starManager;

    private GameObject currentMapInstance;

    private void Start()
    {
        LoadLevel(currentLevelToLoad);
    }

    private void OnEnable()
    {
        GameEvents.OnRestartLevel += ReloadCurrentLevel;
    }

    private void OnDisable()
    {
        GameEvents.OnRestartLevel -= ReloadCurrentLevel;
    }

    public void LoadLevel(LevelData levelData)
    {
        currentLevelToLoad = levelData;

        RespawnMap();

        inventoryManager.InitializeLevel(levelData.startingItems);
        gridManager.InitializeLevel(levelData);
        starManager.InitializeLevel(levelData);

        panelManager.ShowBuildPanel();
    }

    public void ReloadCurrentLevel()
    {
        RespawnMap();
        gridManager.Restart();
        starManager.ReloadLevel();

        panelManager.ShowBuildPanel();
    }

    private void RespawnMap()
    {
        if (currentMapInstance != null) Destroy(currentMapInstance);

        currentMapInstance = Instantiate(currentLevelToLoad.mapPrefab, environmentParent);
        currentMapInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}

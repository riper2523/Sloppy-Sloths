using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private LevelData currentLevelToLoad;
    [SerializeField] private Transform environmentParent; // Where to spawn the map
    
    [SerializeField] private GridManager gridManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PanelManager panelManager;
    [SerializeField] private Transform finishLine;

    private GameObject currentMapInstance;

    void Start()
    {
        // For testing purposes, load the level assigned in the inspector on Start.
        // Later, you will call this from a Level Select Menu.
        if (currentLevelToLoad != null)
        {
            LoadLevel(currentLevelToLoad);
        }
    }

    public void LoadLevel(LevelData levelData)
    {
        // 1. Clean up old level if one exists
        if (currentMapInstance != null) Destroy(currentMapInstance);

        // 2. Spawn the new map
        currentMapInstance = Instantiate(levelData.mapPrefab, environmentParent);
        currentMapInstance.transform.localPosition = Vector3.zero; 
        currentMapInstance.transform.localRotation = Quaternion.identity;

        if (finishLine != null)
        {
            finishLine.position = levelData.finishLinePosition;
            finishLine.localScale = levelData.finishLineScale;

            finishLine.gameObject.SetActive(true);
        }

        // 3. Initialize the Inventory
        inventoryManager.InitializeLevel(levelData.startingItems);

        // 4. Initialize the Grid
        gridManager.InitializeLevel(levelData);


        if (panelManager != null) panelManager.ShowBuildPanel(); 
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

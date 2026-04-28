using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int offsetX = 3;
    [SerializeField] private int offsetY = 3;
    [SerializeField] private int gridSizeX = 5;
    [SerializeField] private int gridSizeY = 5;

    [SerializeField] private Tilemap grid;
    [SerializeField] private Tile tile;
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject gameToggleParent;
    [SerializeField] private GameObject gameTogglePrefab;
    [Header("Listening To")]
    [SerializeField] private VoidEventChannelSO playLevelEvent;
    [SerializeField] private VoidEventChannelSO restartLevelEvent;
    [SerializeField] private GridAnchorEventChannelSO anchorFoundEvent;

    private Dictionary<ActionType, GameToggleScript> actionToggles = new Dictionary<ActionType, GameToggleScript>();
    private Transform vehicleParent;
    private Transform buildCameraTarget;

    struct GridCell
    {
        public PartData partData;
        public int Rotation;
    }

    private GridCell[,] partDataGrid;
    private InputAction clickAction;
    private PartData actPartData;

    private void OnEnable()
    {
        playLevelEvent.OnEventRaised += Build;
        restartLevelEvent.OnEventRaised += Restart;
        anchorFoundEvent.OnEventRaised += InitializeLevel;
    }

    private void OnDisable()
    {
        anchorFoundEvent.OnEventRaised -= InitializeLevel;
        playLevelEvent.OnEventRaised -= Build;
        restartLevelEvent.OnEventRaised -= Restart;
    }

    public void InitializeLevel(GridAnchor anchor)
    {
        vehicleParent = new GameObject("Vehicle").transform;
        clickAction = InputSystem.actions.FindAction("Click");

        LoadLevelSettings(anchor);
    }

    public void LoadLevelSettings(GridAnchor anchor)
    {
        gridSizeX = anchor.gridSizeX;
        gridSizeY = anchor.gridSizeY;
        offsetX = Mathf.RoundToInt(anchor.transform.position.x);
        offsetY = Mathf.RoundToInt(anchor.transform.position.y);

        transform.position = Vector3.zero;

        buildCameraTarget = anchor.transform;
        buildCameraTarget.position += new Vector3(gridSizeX / 2f, gridSizeY / 2f, -10);

        targetGroup.Targets = new List<CinemachineTargetGroup.Target>();
        targetGroup.AddMember(anchor.transform, 1f, 0f);

        partDataGrid = new GridCell[gridSizeX, gridSizeY];

        grid.ClearAllTiles();
        BuildGrid();
    }

    void Update()
    {
        if (clickAction.WasPressedThisFrame())
        {
            if (actPartData != null)
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3Int tilePos = grid.WorldToCell(mouseWorldPos);

                int x = tilePos.x - offsetX;
                int y = tilePos.y - offsetY;

                PlacePart(x, y);
            }
            else
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3Int tilePos = grid.WorldToCell(mouseWorldPos);

                int x = tilePos.x - offsetX;
                int y = tilePos.y - offsetY;

                RemovePart(x, y);
            }
        }
    }
    private void BuildGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x + offsetX, y + offsetY, 0);
                if (partDataGrid[x, y].partData == null)
                {
                    grid.SetTile(tilePosition, tile);
                }
                else
                {
                    grid.SetTile(tilePosition, partDataGrid[x, y].partData.partTile);
                }
            }
        }
    }

    public void SelectPart(PartData partData)
    {
        actPartData = partData;
    }

    public void PlacePart(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY || inventoryManager.TryUsePart(actPartData) == false)
        {
            return;
        }

        int rotation = FindBestRotation(x, y, actPartData);

        if (partDataGrid[x, y].partData != null)
        {
            inventoryManager.AddPart(partDataGrid[x, y].partData, 1);
        }
        partDataGrid[x, y] = new GridCell { partData = actPartData, Rotation = rotation };

        Vector3Int tilePosition = new Vector3Int(x + offsetX, y + offsetY, 0);
        grid.SetTile(tilePosition, actPartData.partTile);
    }
    public void RemovePart(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY || partDataGrid[x, y].partData == null)
        {
            return;
        }

        inventoryManager.AddPart(partDataGrid[x, y].partData, 1);
        partDataGrid[x, y] = new GridCell { partData = null, Rotation = 0 };

        Vector3Int tilePosition = new Vector3Int(x + offsetX, y + offsetY, 0);
        grid.SetTile(tilePosition, tile);
    }

    private int FindBestRotation(int x, int y, PartData partToPlace)
    {
        for (int r = 0; r < 4; r++)
        {
            if (CanAttachWithRotation(x, y, partToPlace, r))
                return r;
        }

        return 0;
    }
    private bool CanAttachWithRotation(int x, int y, PartData part, int rotation)
    {
        bool hasAnyNeighbor = false;
        bool isSuccessfullyAttached = false;

        if (y + 1 < gridSizeY && partDataGrid[x, y + 1].partData != null)
        {
            hasAnyNeighbor = true;
            if (part.HasAttachment(0, rotation) && partDataGrid[x, y + 1].partData.HasAttachment(2, partDataGrid[x, y + 1].Rotation)) isSuccessfullyAttached = true;
        }

        if (x + 1 < gridSizeX && partDataGrid[x + 1, y].partData != null)
        {
            hasAnyNeighbor = true;
            if (part.HasAttachment(1, rotation) && partDataGrid[x + 1, y].partData.HasAttachment(3, partDataGrid[x + 1, y].Rotation)) isSuccessfullyAttached = true;
        }

        if (y - 1 >= 0 && partDataGrid[x, y - 1].partData != null)
        {
            hasAnyNeighbor = true;
            if (part.HasAttachment(2, rotation) && partDataGrid[x, y - 1].partData.HasAttachment(0, partDataGrid[x, y - 1].Rotation)) isSuccessfullyAttached = true;
        }

        if (x - 1 >= 0 && partDataGrid[x - 1, y].partData != null)
        {
            hasAnyNeighbor = true;
            if (part.HasAttachment(3, rotation) && partDataGrid[x - 1, y].partData.HasAttachment(1, partDataGrid[x - 1, y].Rotation)) isSuccessfullyAttached = true;
        }

        if (!hasAnyNeighbor)
            return true;

        return isSuccessfullyAttached;
    }

    public void Build()
    {
        GameObject[,] spawnedParts = new GameObject[gridSizeX, gridSizeY];
        var newTargets = new System.Collections.Generic.List<CinemachineTargetGroup.Target>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (partDataGrid[x, y].partData != null)
                {
                    Vector3 worldPos = grid.CellToWorld(new Vector3Int(x + offsetX, y + offsetY, 0));
                    worldPos.y += grid.cellSize.y / 2;
                    worldPos.x += grid.cellSize.x / 2;


                    Quaternion finalRotation = partDataGrid[x, y].partData.partPrefab.transform.rotation
                                                                 * Quaternion.Euler(0, 0, -partDataGrid[x, y].Rotation * 90);
                    GameObject newPart = Instantiate(partDataGrid[x, y].partData.partPrefab, worldPos, finalRotation);
                    Rigidbody2D rb = newPart.GetComponentInChildren<Rigidbody2D>();

                    if (rb != null)
                    {
                        newTargets.Add(new CinemachineTargetGroup.Target
                        {
                            Object = rb.transform,
                            Weight = 1f,
                            Radius = 1f
                        });
                    }

                    newPart.transform.SetParent(vehicleParent, true);
                    spawnedParts[x, y] = newPart;
                }
            }
        }

        targetGroup.Targets = newTargets;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (spawnedParts[x, y] == null)
                {
                    continue;
                }

                if (x + 1 < gridSizeX && spawnedParts[x + 1, y] != null)
                {
                    TryCreateJoint(x, y, x + 1, y, spawnedParts);
                }

                if (y + 1 < gridSizeY && spawnedParts[x, y + 1] != null)
                {
                    TryCreateJoint(x, y, x, y + 1, spawnedParts);
                }
                PartLogic logic = spawnedParts[x, y].GetComponentInChildren<PartLogic>();
                if (logic != null)
                {
                    foreach (var action in logic.actionReceivers)
                    {
                        if (actionToggles.TryGetValue(action.actionType, out GameToggleScript toggle))
                        {
                            toggle.startEvent.AddListener(() => action.startAction.Invoke());
                            toggle.endEvent.AddListener(() => action.stopAction.Invoke());
                        }
                        else
                        {
                            GameObject toggleGO = Instantiate(gameTogglePrefab, gameToggleParent.transform);
                            GameToggleScript toggleScript = toggleGO.GetComponent<GameToggleScript>();
                            toggleScript.startEvent.AddListener(() => action.startAction.Invoke());
                            toggleScript.endEvent.AddListener(() => action.stopAction.Invoke());
                            actionToggles[action.actionType] = toggleScript;
                        }
                    }
                }
            }
        }

        grid.ClearAllTiles();
    }
    private void TryCreateJoint(int xA, int yA, int xB, int yB, GameObject[,] spawnedParts)
    {
        GridCell cellA = partDataGrid[xA, yA];
        GridCell cellB = partDataGrid[xB, yB];

        bool canAttach = false;

        if (xB > xA)
        {
            bool aHasRight = cellA.partData.HasAttachment(1, cellA.Rotation);
            bool bHasLeft = cellB.partData.HasAttachment(3, cellB.Rotation);

            canAttach = aHasRight && bHasLeft;
        }
        else if (yB > yA)
        {
            bool aHasUp = cellA.partData.HasAttachment(0, cellA.Rotation);
            bool bHasDown = cellB.partData.HasAttachment(2, cellB.Rotation);

            canAttach = aHasUp && bHasDown;
        }

        if (canAttach)
        {
            CreateJoint(spawnedParts[xA, yA], spawnedParts[xB, yB]);
        }
    }
    private void CreateJoint(GameObject partA, GameObject partB)
    {
        Rigidbody2D rbA = partA.GetComponentInChildren<Rigidbody2D>();
        Rigidbody2D rbB = partB.GetComponentInChildren<Rigidbody2D>();

        if (rbA == null || rbB == null) return;

        FixedJoint2D joint = rbA.gameObject.AddComponent<FixedJoint2D>();
        joint.connectedBody = rbB;
    }

    public void Restart()
    {
        foreach (Transform child in vehicleParent)
        {
            Destroy(child.gameObject);
        }
        foreach (var toggle in actionToggles.Values)
        {
            Destroy(toggle.gameObject);
        }
        actionToggles.Clear();

        targetGroup.Targets = new List<CinemachineTargetGroup.Target>();
        targetGroup.AddMember(buildCameraTarget, 1f, 0f);
        BuildGrid();
    }
}

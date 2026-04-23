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
    private Transform vehicleParent;

    struct GridCell
    {
        public PartData partData;
        public int Rotation;
    }

    private GridCell[,] partDataGrid;
    private InputAction clickAction;
    private Vector3 cameraPosition;
    private PartData actPartData;

    void Start()
    {
        GameObject vehicleGO = new GameObject("Vehicle");
        vehicleParent = vehicleGO.transform;

        clickAction = InputSystem.actions.FindAction("Click");
        partDataGrid = new GridCell[gridSizeX, gridSizeY];

        BuildGrid();
    }
    private void BuildGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x + offsetX, y + offsetY, 0);
                grid.SetTile(tilePosition, tile);
            }
        }
    }
    void Update()
    {
        if (clickAction.WasPressedThisFrame() && actPartData != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3Int tilePos = grid.WorldToCell(mouseWorldPos);

            int x = tilePos.x - offsetX;
            int y = tilePos.y - offsetY;

            PlacePart(x, y);
        }
    }

    public void SelectPart(PartData partData)
    {
        actPartData = partData;
    }

    public void PlacePart(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
        {
            return;
        }

        int rotation = FindBestRotation(x, y, actPartData);

        partDataGrid[x, y] = new GridCell { partData = actPartData, Rotation = rotation };

        Vector3Int tilePosition = new Vector3Int(x + offsetX, y + offsetY, 0);
        grid.SetTile(tilePosition, actPartData.partTile);
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
            if (part.HasAttachment(0, rotation)) isSuccessfullyAttached = true;
        }

        if (x + 1 < gridSizeX && partDataGrid[x + 1, y].partData != null)
        {
            hasAnyNeighbor = true;
            if (part.HasAttachment(1, rotation)) isSuccessfullyAttached = true;
        }

        if (y - 1 >= 0 && partDataGrid[x, y - 1].partData != null)
        {
            hasAnyNeighbor = true;
            if (part.HasAttachment(2, rotation)) isSuccessfullyAttached = true;
        }

        if (x - 1 >= 0 && partDataGrid[x - 1, y].partData != null)
        {
            hasAnyNeighbor = true;
            if (part.HasAttachment(3, rotation)) isSuccessfullyAttached = true;
        }

        if (!hasAnyNeighbor)
            return true;

        return isSuccessfullyAttached;
    }

    public void Build()
    {
        cameraPosition = targetGroup.transform.position;
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
        partDataGrid = new GridCell[gridSizeX, gridSizeY];
        grid.ClearAllTiles();

        targetGroup.Targets = new List<CinemachineTargetGroup.Target>();
        targetGroup.AddMember(transform, 1f, 0f);
        BuildGrid();
    }
}

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int offsetX = 3;
    [SerializeField]
    private int offsetY = 3;

    [SerializeField]
    private int gridSizeX = 5;
    [SerializeField]
    private int gridSizeY = 5;
    [SerializeField]
    private Tilemap grid;
    [SerializeField]
    private Tile tile;

    [SerializeField]
    private CinemachineTargetGroup targetGroup;
    private Transform vehicleParent;

    struct GridCell
    {
        public PartData partData;
        public int Rotation;
    }

    private GridCell[,] partDataGrid;
    private InputAction clickAction;

    private Vector3 cameraPosition;
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
        if (clickAction.WasPressedThisFrame())
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3Int tilePos = grid.WorldToCell(mouseWorldPos);
            int x = tilePos.x - offsetX;
            int y = tilePos.y - offsetY;
            if (actPartData != null)
            {
                PlacePart(x, y);
            }
        }
    }

    private PartData actPartData;
    public void PlacePart(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
        {
            // Debug.LogError("Invalid position or no part selected.");
            return;
        }
        int rotation = 0;

        bool hasAbove = y + 1 < gridSizeY && partDataGrid[x, y + 1].partData != null;
        bool hasBelow = y - 1 >= 0 && partDataGrid[x, y - 1].partData != null;
        bool hasLeft = x - 1 >= 0 && partDataGrid[x - 1, y].partData != null;
        bool hasRight = x + 1 < gridSizeX && partDataGrid[x + 1, y].partData != null;

        if (!hasAbove && actPartData.name.Contains("Wheel"))
        {
            rotation++;
            if (!hasLeft)
            {
                rotation++;
                if (!hasBelow)
                {
                    rotation++;
                    if (!hasRight)
                    {
                        rotation++;
                    }
                }
            }
        }
        rotation %= 4;

        partDataGrid[x, y] = new GridCell { partData = actPartData, Rotation = rotation };
        Vector3Int tilePosition = new Vector3Int(x + offsetX, y + offsetY, 0);
        grid.SetTile(tilePosition, actPartData.partTile);
    }
    public void SelectPart(PartData partData)
    {
        actPartData = partData;
    }

    public void Build()
    {
        cameraPosition = targetGroup.transform.position;
        // 1. Tworzymy tymczasową tablicę na obiekty fizyczne
        GameObject[,] spawnedParts = new GameObject[gridSizeX, gridSizeY];
        var newTargets = new System.Collections.Generic.List<CinemachineTargetGroup.Target>();
        // 2. PIERWSZA PĘTLA: Spawnowanie wszystkich klocków
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (partDataGrid[x, y].partData != null)
                {
                    Vector3 worldPos = grid.CellToWorld(new Vector3Int(x + offsetX, y + offsetY, 0));
                    worldPos.y += grid.cellSize.y / 2;
                    worldPos.x += grid.cellSize.x / 2;

                    GameObject newPart = Instantiate(partDataGrid[x, y].partData.partPrefab, worldPos, partDataGrid[x, y].partData.partPrefab.transform.rotation * Quaternion.Euler(0, 0, partDataGrid[x, y].Rotation * 90));

                    Rigidbody2D rb = newPart.GetComponentInChildren<Rigidbody2D>();
                    if (rb != null)
                    {
                        newTargets.Add(new CinemachineTargetGroup.Target
                        {
                            Object = rb.transform,
                            Weight = 1f, // Jak bardzo ten klocek przyciąga kamerę
                            Radius = 1f  // Obszar wokół klocka, który kamera ma widzieć
                        });
                    }

                    newPart.transform.SetParent(vehicleParent, true);
                    spawnedParts[x, y] = newPart;
                }
            }
        }
        targetGroup.Targets = newTargets;
        // 3. DRUGA PĘTLA: Łączenie sąsiadów
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (spawnedParts[x, y] == null) continue;

                // Sprawdzamy sąsiada z prawej (x + 1)
                if (x + 1 < gridSizeX && spawnedParts[x + 1, y] != null)
                {
                    CreateJoint(spawnedParts[x, y], spawnedParts[x + 1, y]);
                }

                // Sprawdzamy sąsiada u góry (y + 1)
                if (y + 1 < gridSizeY && spawnedParts[x, y + 1] != null)
                {
                    CreateJoint(spawnedParts[x, y], spawnedParts[x, y + 1]);
                }
            }
        }

        grid.ClearAllTiles(); // Czyścimy siatkę podglądu
    }

    // Pomocnicza metoda do tworzenia Jointa
    private void CreateJoint(GameObject partA, GameObject partB)
    {
        // 1. Szukamy Rigidbody2D gdziekolwiek w strukturze obu klocków
        Rigidbody2D rbA = partA.GetComponentInChildren<Rigidbody2D>();
        Rigidbody2D rbB = partB.GetComponentInChildren<Rigidbody2D>();

        // 2. Jeśli któryś klockek nie ma fizyki, nie możemy ich połączyć
        if (rbA == null || rbB == null) return;

        // 3. Dodajemy Joint bezpośrednio do obiektu, który MA RigidbodyA
        // To gwarantuje, że Joint będzie działał, nawet jeśli RB jest w dziecku
        FixedJoint2D joint = rbA.gameObject.AddComponent<FixedJoint2D>();

        // 4. Łączymy go z RigidbodyB
        joint.connectedBody = rbB;

        // 5. Opcje stabilności
        // joint.enableCollision = false; // Klocki nie wchodzą ze sobą w kolizję (lepsza wydajność)

        // 6. Opcja zniszczalności
        // joint.breakForce = 500f; // Jeśli chcesz, żeby pojazd mógł pękać
    }

    public void Restart()
    {
        foreach (Transform child in vehicleParent)
        {
            Destroy(child.gameObject);
        }
        partDataGrid = new GridCell[gridSizeX, gridSizeY];
        grid.ClearAllTiles();
        targetGroup.Targets = new System.Collections.Generic.List<CinemachineTargetGroup.Target>();
        targetGroup.AddMember(transform, 1f, 0f);
        BuildGrid();
    }
}

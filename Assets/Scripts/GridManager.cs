using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int offsetX = 0;
    [SerializeField]
    private int offsetY = 0;

    [SerializeField]
    private int gridSizeX = 5;
    [SerializeField]
    private int gridSizeY = 5;
    [SerializeField]
    private Tilemap grid;
    [SerializeField]
    private Tile tile;

    private PartData[,] partDataGrid;

    private InputAction clickAction;

    void Start()
    {
        clickAction = InputSystem.actions.FindAction("Click");
        partDataGrid = new PartData[gridSizeX, gridSizeY];
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
            PlacePart(x, y);
        }
    }

    private PartData actPartData;
    public void PlacePart(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY || actPartData == null)
        {
            Debug.LogError("Invalid position or no part selected.");
            return;
        }
        partDataGrid[x, y] = actPartData;
        Vector3Int tilePosition = new Vector3Int(x + offsetX, y + offsetY, 0);
        grid.SetTile(tilePosition, actPartData.partTile);
    }
    public void SelectPart(PartData partData)
    {
        actPartData = partData;
    }
}

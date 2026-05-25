using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("Listening To")]
    [SerializeField] private VoidEventChannelSO playLevelEvent;
    [SerializeField] private VoidEventChannelSO restartLevelEvent;

    [SerializeField] private int offsetX = 3;
    [SerializeField] private int offsetY = 3;
    [SerializeField] private int gridSizeX = 5;
    [SerializeField] private int gridSizeY = 5;

    [SerializeField] private Tilemap partsTilemap;
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tile backgroundTile;
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject gameToggleParent;
    [SerializeField] private GameObject gameTogglePrefab;

    private Dictionary<ActionType, GameObject> actionToggles = new Dictionary<ActionType, GameObject>();
    private Dictionary<ActionType, int> activeActions = new Dictionary<ActionType, int>();


    private Transform vehicleParent;
    private Transform buildCameraTarget;

    public class PartInstance
    {
        public PartData partData;
        public int Rotation;
    }

    struct GridCell
    {
        public Dictionary<int, PartInstance> parts;
    }
    private GridCell[,] partDataGrid;
    private PartData actPartData;

    private void OnEnable()
    {
        playLevelEvent.OnEventRaised += Build;
        restartLevelEvent.OnEventRaised += Restart;
    }

    private void OnDisable()
    {
        playLevelEvent.OnEventRaised -= Build;
        restartLevelEvent.OnEventRaised -= Restart;
    }

    public void InitializeLevel(GridAnchor anchor)
    {
        vehicleParent = new GameObject("Vehicle").transform;
        LoadLevelSettings(anchor);
    }

    public void LoadLevelSettings(GridAnchor anchor)
    {
        gridSizeX = anchor.gridSizeX;
        gridSizeY = anchor.gridSizeY;
        offsetX = Mathf.RoundToInt(anchor.transform.position.x);
        offsetY = Mathf.RoundToInt(anchor.transform.position.y);

        transform.position = Vector3.zero;

        if (buildCameraTarget == null)
        {
            buildCameraTarget = new GameObject("BuildCameraTarget").transform;
            buildCameraTarget.SetParent(transform);
        }
        Vector3 gridCenter = new Vector3(offsetX + gridSizeX / 2f, offsetY + gridSizeY / 2f, 0);
        buildCameraTarget.position = gridCenter;

        targetGroup.Targets = new List<CinemachineTargetGroup.Target>();
        targetGroup.AddMember(buildCameraTarget, 1f, 0f);

        partDataGrid = new GridCell[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                partDataGrid[x, y].parts = new Dictionary<int, PartInstance>();
            }
        }

        backgroundTilemap.ClearAllTiles();
        partsTilemap.ClearAllTiles();

        BuildGrid();
    }

    public void PartActivated(ActionType actionType)
    {
        if (activeActions.ContainsKey(actionType))
        {
            activeActions[actionType]++;
        }
        else
        {
            activeActions[actionType] = 1;
        }
        if (activeActions[actionType] == 1 && actionToggles.TryGetValue(actionType, out GameObject toggle))
        {
            toggle.GetComponent<Toggle>().SetIsOnWithoutNotify(true);
        }
    }
    public void PartDeactivated(ActionType actionType)
    {
        if (activeActions.ContainsKey(actionType))
        {
            activeActions[actionType]--;
            if (activeActions[actionType] <= 0)
            {
                activeActions[actionType] = 0;
                if (actionToggles.TryGetValue(actionType, out GameObject toggle))
                {
                    toggle.GetComponent<Toggle>().SetIsOnWithoutNotify(false);
                }
            }
        }
    }
    public void OnSingleClick(Vector3Int tilePos)
    {
        int x = tilePos.x - offsetX;
        int y = tilePos.y - offsetY;
        if (actPartData != null)
        {
            PlacePart(x, y);
        }
        else
        {
            RemovePart(x, y);
        }
    }
    public void OnDoubleClick(Vector3Int tilePos)
    {
        int x = tilePos.x - offsetX;
        int y = tilePos.y - offsetY;
        RotatePart(x, y);
    }
    private void BuildGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3Int bgPos = new Vector3Int(x + offsetX, y + offsetY, 0);
                backgroundTilemap.SetTile(bgPos, backgroundTile);

                UpdateTilemapForCell(x, y);
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

        var cellParts = partDataGrid[x, y].parts;
        List<int> layersToRemove = new List<int>();

        foreach (var kvp in cellParts)
        {
            PartData existingPart = kvp.Value.partData;

            if (existingPart.layer == actPartData.layer ||
                !actPartData.acceptedLayers.Contains(existingPart.layer) ||
                !existingPart.acceptedLayers.Contains(actPartData.layer))
            {
                layersToRemove.Add(kvp.Key);
            }
        }

        foreach (int layerId in layersToRemove)
        {
            inventoryManager.AddPart(cellParts[layerId].partData, 1);
            cellParts.Remove(layerId);
        }
        int rotation = FindBestRotation(x, y, actPartData);
        cellParts[actPartData.layer] = new PartInstance { partData = actPartData, Rotation = rotation };

        UpdateTilemapForCell(x, y);
    }
    public void RemovePart(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
        {
            return;
        }
        var cellParts = partDataGrid[x, y].parts;
        if (cellParts.Count == 0) return;

        int topLayer = -1;
        foreach (int layerId in cellParts.Keys)
        {
            if (layerId > topLayer) topLayer = layerId;
        }

        inventoryManager.AddPart(cellParts[topLayer].partData, 1);
        cellParts.Remove(topLayer);

        UpdateTilemapForCell(x, y);
    }
    public void RotatePart(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
        {
            return;
        }

        var cellParts = partDataGrid[x, y].parts;
        if (cellParts.Count == 0) return;

        int topLayer = -1;
        foreach (int layerId in cellParts.Keys)
        {
            if (layerId > topLayer) topLayer = layerId;
        }

        int newRotation = (cellParts[topLayer].Rotation + 1) % 4;
        cellParts[topLayer].Rotation = newRotation;

        UpdateTilemapForCell(x, y);
    }
    private void UpdateTilemapForCell(int x, int y)
    {
        for (int z = 0; z < 10; z++)
        {
            Vector3Int clearPos = new Vector3Int(x + offsetX, y + offsetY, z);
            partsTilemap.SetTile(clearPos, null);
            partsTilemap.SetTransformMatrix(clearPos, Matrix4x4.identity);
        }

        var cellParts = partDataGrid[x, y].parts;

        if (cellParts == null || cellParts.Count == 0)
        {
            return;
        }

        foreach (var kvp in cellParts)
        {
            int layerId = kvp.Key;
            PartInstance pInst = kvp.Value;

            Vector3Int tilePosition = new Vector3Int(x + offsetX, y + offsetY, layerId);

            partsTilemap.SetTile(tilePosition, pInst.partData.partTile);
            ApplyRotation(tilePosition, pInst.Rotation);
        }
    }
    private void ApplyRotation(Vector3Int tilePosition, int rotationIndex)
    {
        partsTilemap.SetTileFlags(tilePosition, TileFlags.None);
        float angle = rotationIndex * -90f;
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one);
        partsTilemap.SetTransformMatrix(tilePosition, matrix);
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
        int l = part.layer;

        if (y + 1 < gridSizeY && partDataGrid[x, y + 1].parts.ContainsKey(l))
        {
            hasAnyNeighbor = true;
            var neighbor = partDataGrid[x, y + 1].parts[l];
            if (part.HasAttachment(0, rotation) && neighbor.partData.HasAttachment(2, neighbor.Rotation)) isSuccessfullyAttached = true;
        }

        if (x + 1 < gridSizeX && partDataGrid[x + 1, y].parts.ContainsKey(l))
        {
            hasAnyNeighbor = true;
            var neighbor = partDataGrid[x + 1, y].parts[l];
            if (part.HasAttachment(1, rotation) && neighbor.partData.HasAttachment(3, neighbor.Rotation)) isSuccessfullyAttached = true;
        }

        if (y - 1 >= 0 && partDataGrid[x, y - 1].parts.ContainsKey(l))
        {
            hasAnyNeighbor = true;
            var neighbor = partDataGrid[x, y - 1].parts[l];
            if (part.HasAttachment(2, rotation) && neighbor.partData.HasAttachment(0, neighbor.Rotation)) isSuccessfullyAttached = true;
        }

        if (x - 1 >= 0 && partDataGrid[x - 1, y].parts.ContainsKey(l))
        {
            hasAnyNeighbor = true;
            var neighbor = partDataGrid[x - 1, y].parts[l];
            if (part.HasAttachment(3, rotation) && neighbor.partData.HasAttachment(1, neighbor.Rotation)) isSuccessfullyAttached = true;
        }

        if (!hasAnyNeighbor) return true;
        return isSuccessfullyAttached;
    }

    public void Build()
    {
        backgroundTilemap.gameObject.SetActive(false);

        Dictionary<int, GameObject>[,] spawnedParts = new Dictionary<int, GameObject>[gridSizeX, gridSizeY];
        var newTargets = new List<CinemachineTargetGroup.Target>();

        // 1. SPAWNOWANIE
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                spawnedParts[x, y] = new Dictionary<int, GameObject>();
                var cellParts = partDataGrid[x, y].parts;

                if (cellParts.Count == 0) continue;

                // Szukamy najniższej warstwy (najbardziej bazowej) w TEJ konkretnej kratce
                int minLayerInCell = int.MaxValue;
                foreach (int l in cellParts.Keys)
                {
                    if (l < minLayerInCell) minLayerInCell = l;
                }

                foreach (var kvp in cellParts)
                {
                    int layer = kvp.Key;
                    PartInstance pInst = kvp.Value;

                    Vector3 worldPos = partsTilemap.CellToWorld(new Vector3Int(x + offsetX, y + offsetY, 0));
                    worldPos.y += partsTilemap.cellSize.y / 2;
                    worldPos.x += partsTilemap.cellSize.x / 2;

                    Quaternion finalRotation = pInst.partData.partPrefab.transform.rotation
                                             * Quaternion.Euler(0, 0, pInst.Rotation * -90f);

                    GameObject newPart = Instantiate(pInst.partData.partPrefab, worldPos, finalRotation);

                    // WYŁĄCZANIE KOLIZJI DLA WYŻSZYCH WARSTW
                    if (layer > minLayerInCell)
                    {
                        // Szukamy wszystkich colliderów na tym klocku i wyłączamy je
                        Collider2D[] colliders = newPart.GetComponentsInChildren<Collider2D>();
                        foreach (Collider2D col in colliders)
                        {
                            col.isTrigger = true;
                        }
                    }

                    Rigidbody2D rb = newPart.GetComponentInChildren<Rigidbody2D>();
                    if (rb != null)
                    {
                        newTargets.Add(new CinemachineTargetGroup.Target { Object = rb.transform, Weight = 1f, Radius = 1f });
                    }

                    newPart.transform.SetParent(vehicleParent, true);
                    spawnedParts[x, y][layer] = newPart;

                    // Obsługa logiki
                    PartLogic logic = newPart.GetComponentInChildren<PartLogic>();
                    if (logic != null)
                    {
                        logic.gridPosition = new Vector3Int(x, y, layer);
                        logic.jointBreakEvent.AddListener(HandleJointBreak);

                        foreach (var action in logic.actionReceivers)
                        {
                            action.startAction.AddListener(() => PartActivated(action.actionType));
                            action.stopAction.AddListener(() => PartDeactivated(action.actionType));
                            if (actionToggles.TryGetValue(action.actionType, out GameObject toggle))
                            {
                                GameToggleScript toggleScript = toggle.GetComponent<GameToggleScript>();
                                toggleScript.startEvent.AddListener(() => action.startAction.Invoke());
                                toggleScript.endEvent.AddListener(() => action.stopAction.Invoke());
                            }
                            else
                            {
                                GameObject toggleGO = Instantiate(gameTogglePrefab, gameToggleParent.transform);
                                GameToggleScript toggleScript = toggleGO.GetComponent<GameToggleScript>();
                                toggleScript.startEvent.AddListener(() => action.startAction.Invoke());
                                toggleScript.endEvent.AddListener(() => action.stopAction.Invoke());
                                actionToggles[action.actionType] = toggleGO;
                            }
                        }
                    }
                }
            }
        }

        targetGroup.Targets = newTargets;

        // 2. TWORZENIE JOINTÓW (ŁĄCZENIE)
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                var currentCellParts = spawnedParts[x, y];
                if (currentCellParts.Count == 0) continue;

                // A) Łączenie Wewnętrzne (Części na tej samej kratce łączą się ze sobą)
                if (currentCellParts.Count > 1)
                {
                    List<GameObject> partsInCell = new List<GameObject>(currentCellParts.Values);
                    for (int i = 1; i < partsInCell.Count; i++)
                    {
                        PartLogic logicA = partsInCell[0].GetComponentInChildren<PartLogic>();
                        PartLogic logicB = partsInCell[i].GetComponentInChildren<PartLogic>();
                        CreateJoint(logicA, logicB, Direction.Forward, 50000f);
                    }
                }

                // B) Łączenie z sąsiadami (TYLKO jeśli mają ten sam Layer)
                foreach (int layer in currentCellParts.Keys)
                {
                    if (x + 1 < gridSizeX && spawnedParts[x + 1, y].ContainsKey(layer))
                    {
                        TryCreateJointLayer(x, y, x + 1, y, layer, spawnedParts);
                    }

                    if (y + 1 < gridSizeY && spawnedParts[x, y + 1].ContainsKey(layer))
                    {
                        TryCreateJointLayer(x, y, x, y + 1, layer, spawnedParts);
                    }
                }
            }
        }

        partsTilemap.ClearAllTiles();
        RecalculatePartsState();
    }
    private void TryCreateJointLayer(int xA, int yA, int xB, int yB, int layer, Dictionary<int, GameObject>[,] spawnedParts)
    {
        PartInstance pA = partDataGrid[xA, yA].parts[layer];
        PartInstance pB = partDataGrid[xB, yB].parts[layer];

        bool canAttach = false;
        Direction dirFromAToB = default;

        if (xB > xA)
        {
            bool aHasRight = pA.partData.HasAttachment(1, pA.Rotation);
            bool bHasLeft = pB.partData.HasAttachment(3, pB.Rotation);
            if (aHasRight && bHasLeft)
            {
                canAttach = true;
                dirFromAToB = Direction.Right;
            }
        }
        else if (yB > yA)
        {
            bool aHasUp = pA.partData.HasAttachment(0, pA.Rotation);
            bool bHasDown = pB.partData.HasAttachment(2, pB.Rotation);
            if (aHasUp && bHasDown)
            {
                canAttach = true;
                dirFromAToB = Direction.Up;
            }
        }

        if (canAttach)
        {
            PartLogic partLogicA = spawnedParts[xA, yA][layer].GetComponentInChildren<PartLogic>();
            PartLogic partLogicB = spawnedParts[xB, yB][layer].GetComponentInChildren<PartLogic>();
            float aStrength = partLogicA != null ? partLogicA.actualJointStrength : 0f;
            float bStrength = partLogicB != null ? partLogicB.actualJointStrength : 0f;
            float strength = (aStrength + bStrength) / 2f;
            CreateJoint(partLogicA, partLogicB, dirFromAToB, strength);
        }
    }
    private void CreateJoint(PartLogic logicA, PartLogic logicB, Direction dirFromAToB, float strength = 100f)
    {
        if (logicA == null || logicB == null) return;

        Rigidbody2D rbA = logicA.GetComponentInChildren<Rigidbody2D>();
        Rigidbody2D rbB = logicB.GetComponentInChildren<Rigidbody2D>();

        if (rbA == null || rbB == null) return;

        FixedJoint2D joint = rbA.gameObject.AddComponent<FixedJoint2D>();
        joint.connectedBody = rbB;
        joint.breakForce = strength;

        logicA.AddConnection(dirFromAToB, logicB, joint);
        logicB.AddConnection(dirFromAToB.Opposite(), logicA, null);
    }
    private void HandleJointBreak(Vector3Int partPos, Direction breakDir)
    {
        // Debug.Log($"Joint broken at {partPos} towards {breakDir}");
        RecalculatePartsState();
    }
    public void RecalculatePartsState()
    {
        PartLogic[] activeParts = vehicleParent.GetComponentsInChildren<PartLogic>();

        foreach (var part in activeParts)
        {
            part.ResetPart();
        }
        foreach (var part in activeParts)
        {
            part.ActivateEffects();
        }
        foreach (var part in activeParts)
        {
            part.ApplyModifiers();
        }
    }
    public void Restart()
    {
        backgroundTilemap.gameObject.SetActive(true);
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

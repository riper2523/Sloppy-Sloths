using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
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
    public int brokenPartLayerID = 8;

    private Dictionary<ActionType, GameObject> actionToggles = new Dictionary<ActionType, GameObject>();
    private Dictionary<ActionType, int> activeActions = new Dictionary<ActionType, int>();
    private Dictionary<int, GameObject>[,] activeSpawnedParts;

    private Transform vehicleParent;
    private Transform buildCameraTarget;

    public class PartInstance
    {
        public PartData partData;
        public int Rotation;
        public int AimRotation;
    }

    struct GridCell
    {
        public Dictionary<int, PartInstance> parts;
    }
    private GridCell[,] partDataGrid;
    private PartData actPartData;
    private LevelData pendingRestoreLevelData;

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
    public void ClearPartGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                while (partDataGrid[x, y].parts.Count > 0)
                {
                    RemovePart(x, y);
                }
            }
        }
        partsTilemap.ClearAllTiles();
    }

    public void InitializeLevel(GridAnchor anchor)
    {
        vehicleParent = new GameObject("Vehicle").transform;
        LoadLevelSettings(anchor);

        if (pendingRestoreLevelData != null)
        {
            LevelData levelToRestore = pendingRestoreLevelData;
            pendingRestoreLevelData = null;
            RequestRestoreForLevel(levelToRestore);
        }
    }

    public void RequestRestoreForLevel(LevelData levelData)
    {
        if (partDataGrid == null)
        {
            pendingRestoreLevelData = levelData;
            return;
        }

        if (SaveManager.Instance != null &&
            SaveManager.Instance.TryGetLevelData(levelData.uniqueID, out LevelSaveData levelSave)
            && levelSave.lastAttemptedGrid != null
            && levelSave.lastAttemptedGrid.cells.Count > 0)
        {
            RestoreGridState(levelSave.lastAttemptedGrid, levelData.startingItems);
        }
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
        RotateAim(x, y);
    }
    public void OnRightClick(Vector3Int tilePos)
    {
        int x = tilePos.x - offsetX;
        int y = tilePos.y - offsetY;
        RotateStructure(x, y);
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
    
    public GridSaveData ExportGridState()
    {
        GridSaveData gridData = new GridSaveData();
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                var cellParts = partDataGrid[x, y].parts;
                if (cellParts.Count > 0)
                {
                    CellSaveData cellData = new CellSaveData { x = x, y = y };
                    foreach (var kvp in cellParts)
                    {
                        PartSaveData partData = new PartSaveData
                        {
                            layer = kvp.Key,
                            rotation = kvp.Value.Rotation,
                            partID = kvp.Value.partData.uniqueID
                        };
                        cellData.parts.Add(partData);
                    }
                    gridData.cells.Add(cellData);
                }
            }
        }
        return gridData;
    }

    public void RestoreGridState(GridSaveData savedData, Inventory startingInventory)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                partDataGrid[x, y].parts.Clear();
            }
        }
        partsTilemap.ClearAllTiles();

        foreach (var cellSave in savedData.cells)
        {
            if (cellSave.x < 0 || cellSave.x >= gridSizeX || cellSave.y < 0 || cellSave.y >= gridSizeY)
                continue;

            foreach (var partSave in cellSave.parts)
            {
                PartData partDataRef = null;
                
                foreach(var part in startingInventory.itemsMap.Keys)
                {
                    if (part.uniqueID == partSave.partID)
                    {
                        partDataRef = part;
                        break;
                    }
                }
                
                if (partDataRef != null && inventoryManager.TryUsePart(partDataRef))
                {
                    partDataGrid[cellSave.x, cellSave.y].parts[partSave.layer] = new PartInstance 
                    { 
                        partData = partDataRef, 
                        Rotation = partSave.rotation 
                    };
                }
            }
            UpdateTilemapForCell(cellSave.x, cellSave.y);
        }
    }

    public void RotateStructure(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY) return;

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

    public void RotateAim(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY) return;

        var cellParts = partDataGrid[x, y].parts;
        if (cellParts.Count == 0) return;

        int topLayer = -1;
        foreach (int layerId in cellParts.Keys)
        {
            if (layerId > topLayer) topLayer = layerId;
        }

        PartInstance pInst = cellParts[topLayer];

        if (pInst.partData.isAimable && pInst.partData.aimStates != null && pInst.partData.aimStates.Length > 0)
        {
            pInst.AimRotation = (pInst.AimRotation + 1) % pInst.partData.aimStates.Length;
            UpdateTilemapForCell(x, y);
        }
        else
        {
            RotateStructure(x, y);
        }
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

            TileBase tileToDraw = pInst.partData.partTile;

            if (pInst.partData.isAimable && pInst.partData.aimStates != null && pInst.partData.aimStates.Length > 0)
            {
                tileToDraw = pInst.partData.aimStates[pInst.AimRotation].visualTile;
            }

            partsTilemap.SetTile(tilePosition, tileToDraw);

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
                    if (pInst.partData.isAimable && pInst.partData.aimStates != null && pInst.partData.aimStates.Length > 0)
                    {
                        AimablePartComponent aimScript = newPart.GetComponentInChildren<AimablePartComponent>();
                        if (aimScript != null)
                        {
                            float exactAngle = pInst.partData.aimStates[pInst.AimRotation].angle;
                            aimScript.SetAimRotation(exactAngle);
                        }
                    }
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
                    List<GameObject> partsInCell = currentCellParts.OrderBy(kvp => -kvp.Key).Select(kvp => kvp.Value).ToList();
                    for (int i = 1; i < partsInCell.Count; i++)
                    {
                        PartLogic logicA = partsInCell[0].GetComponentInChildren<PartLogic>();
                        PartLogic logicB = partsInCell[i].GetComponentInChildren<PartLogic>();
                        float aStrength = logicA != null ? logicA.actualJointStrength : 0f;
                        float bStrength = logicB != null ? logicB.actualJointStrength : 0f;
                        float strength = (aStrength + bStrength) / 2f;
                        CreateJoint(logicA, logicB, Direction.Forward, strength);
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
        activeSpawnedParts = spawnedParts;
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

    public void RecalculatePartsState()
    {
        targetGroup.Targets = new List<CinemachineTargetGroup.Target>();

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
        foreach (var part in activeParts)
        {
            if (part.gameObject.tag == "Sloth")
            {
                targetGroup.AddMember(part.GetComponentInChildren<Rigidbody2D>().transform, 1f, 0f);
            }
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
    private void RefreshEntireTilemap()
    {
        partsTilemap.ClearAllTiles();
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                UpdateTilemapForCell(x, y);
            }
        }
    }
    [ContextMenu("Rotate Clockwise")]
    public void RotateAllPartsClockwise() => RotateCar(true);

    [ContextMenu("Rotate Counter-Clockwise")]
    public void RotateAllPartsCounterClockwise() => RotateCar(false);

    private struct PendingMove
    {
        public int x;
        public int y;
        public Dictionary<int, PartInstance> parts;
    }

    private void RotateCar(bool clockwise)
    {
        // 1. Znajdź Bounding Box (obszar, który zajmuje pojazd)
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        bool hasParts = false;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (partDataGrid[x, y].parts.Count > 0)
                {
                    hasParts = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (!hasParts) return;

        // 2. Wylicz wymiary obecnego prostokąta konstrukcji
        int w = maxX - minX + 1;
        int h = maxY - minY + 1;

        // 3. Oblicz nowy lewy dolny róg tak, aby po zmianie wymiarów (w na h, h na w) środek auta pozostał w tym samym miejscu.
        // Używamy wyłącznie bezpiecznej matematyki całkowitoliczbowej.
        int newMinX = minX + (w / 2) - (h / 2);
        int newMinY = minY + (h / 2) - (w / 2);

        // 4. Przygotuj listę ruchów, żeby nie nadpisywać siatki w trakcie czytania
        List<PendingMove> pendingMoves = new List<PendingMove>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (partDataGrid[x, y].parts.Count > 0)
                {
                    // Relatywne pozycje wewnątrz naszego "prostokąta" od (0,0)
                    int localX = x - minX;
                    int localY = y - minY;

                    int newLocalX, newLocalY;

                    // Czysta transformacja macierzy - ZAWSZE trafia perfekcyjnie w kratkę
                    if (clockwise)
                    {
                        newLocalX = localY;
                        newLocalY = w - 1 - localX;
                    }
                    else
                    {
                        newLocalX = h - 1 - localY;
                        newLocalY = localX;
                    }

                    int newX = newMinX + newLocalX;
                    int newY = newMinY + newLocalY;

                    // Sprawdzenie kolizji z krawędzią mapy.
                    // Jeśli obrót wywaliłby choćby czubek zderzaka poza planszę - anulujemy cały proces obrotu.
                    if (newX < 0 || newX >= gridSizeX || newY < 0 || newY >= gridSizeY)
                    {
                        Debug.LogWarning("Obrót zablokowany: Konstrukcja wyszłaby poza grid.");
                        return;
                    }

                    // Klonujemy części do nowej kratki i dodajemy rotację
                    Dictionary<int, PartInstance> rotatedParts = new Dictionary<int, PartInstance>();
                    foreach (var kvp in partDataGrid[x, y].parts)
                    {
                        int rotOffset = clockwise ? 1 : 3;
                        rotatedParts[kvp.Key] = new PartInstance
                        {
                            partData = kvp.Value.partData,
                            Rotation = (kvp.Value.Rotation + rotOffset) % 4
                        };
                    }

                    pendingMoves.Add(new PendingMove { x = newX, y = newY, parts = rotatedParts });
                }
            }
        }

        // 5. Jeśli kod dotarł tutaj, obrót jest bezpieczny na 100%. Generujemy czystą siatkę.
        GridCell[,] newGrid = new GridCell[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                newGrid[x, y].parts = new Dictionary<int, PartInstance>();
            }
        }

        // 6. Wklejamy przekalkulowane, obrócone klocki na nową siatkę.
        foreach (var move in pendingMoves)
        {
            newGrid[move.x, move.y].parts = move.parts;
        }

        // 7. Zastępujemy stare dane i odświeżamy grafikę.
        partDataGrid = newGrid;
        RefreshEntireTilemap();
    }
    [ContextMenu("Shift Right")] public void ShiftRight() => ShiftGrid(1, 0);
    [ContextMenu("Shift Left")] public void ShiftLeft() => ShiftGrid(-1, 0);
    [ContextMenu("Shift Up")] public void ShiftUp() => ShiftGrid(0, 1);
    [ContextMenu("Shift Down")] public void ShiftDown() => ShiftGrid(0, -1);

    public void ShiftGrid(int dx, int dy)
    {
        // 1. Sprawdź, czy przesunięcie nie wyrzuci klocków poza siatkę
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (partDataGrid[x, y].parts.Count > 0)
                {
                    int newX = x + dx;
                    int newY = y + dy;

                    // Jeśli choć jeden klocek wyjdzie poza zakres, przerywamy operację
                    if (newX < 0 || newX >= gridSizeX || newY < 0 || newY >= gridSizeY)
                    {
                        return;
                    }
                }
            }
        }

        // 2. Przygotuj czystą, nową siatkę
        GridCell[,] newGrid = new GridCell[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                newGrid[x, y].parts = new Dictionary<int, PartInstance>();
            }
        }

        // 3. Skopiuj dane ze starych pozycji na nowe
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (partDataGrid[x, y].parts.Count > 0)
                {
                    newGrid[x + dx, y + dy].parts = partDataGrid[x, y].parts;
                }
            }
        }

        // 4. Zastąp starą siatkę nową i odśwież grafikę
        partDataGrid = newGrid;
        RefreshEntireTilemap();
    }
    [ContextMenu("Mirror Horizontally")]
    public void MirrorAllPartsHorizontally() => MirrorCar(true);

    [ContextMenu("Mirror Vertically")]
    public void MirrorAllPartsVertically() => MirrorCar(false);

    private void MirrorCar(bool horizontal)
    {
        // 1. Znajdź Bounding Box (obszar zajmowany przez auto)
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        bool hasParts = false;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (partDataGrid[x, y].parts.Count > 0)
                {
                    hasParts = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (!hasParts) return;

        List<PendingMove> pendingMoves = new List<PendingMove>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (partDataGrid[x, y].parts.Count > 0)
                {
                    int newX = horizontal ? (minX + maxX - x) : x;
                    int newY = horizontal ? y : (minY + maxY - y);

                    // 3. Klonowanie i odwracanie kierunku klocków (Obrót strukturalny)
                    Dictionary<int, PartInstance> mirroredParts = new Dictionary<int, PartInstance>();
                    foreach (var kvp in partDataGrid[x, y].parts)
                    {
                        int oldRot = kvp.Value.Rotation;
                        int newRot = oldRot;

                        // NAPRAWIONA MATEMATYKA ROTACJI
                        if (horizontal)
                        {
                            // Lustro Poziome (X): Zamieniamy Prawo(1) z Lewo(3).
                            if (oldRot == 1) newRot = 3;
                            else if (oldRot == 3) newRot = 1;
                        }
                        else
                        {
                            // Lustro Pionowe (Y): Zamieniamy Górę(0) z Dołem(2).
                            if (oldRot == 0) newRot = 2;
                            else if (oldRot == 2) newRot = 0;
                        }

                        mirroredParts[kvp.Key] = new PartInstance
                        {
                            partData = kvp.Value.partData,
                            Rotation = newRot
                        };
                    }

                    pendingMoves.Add(new PendingMove { x = newX, y = newY, parts = mirroredParts });
                }
            }
        }

        // 4. Generujemy czystą siatkę.
        GridCell[,] newGrid = new GridCell[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                newGrid[x, y].parts = new Dictionary<int, PartInstance>();
            }
        }

        // 5. Wklejamy odbite klocki na nową siatkę.
        foreach (var move in pendingMoves)
        {
            newGrid[move.x, move.y].parts = move.parts;
        }

        // 6. Zastępujemy stare dane i odświeżamy grafikę.
        partDataGrid = newGrid;
        RefreshEntireTilemap();
    }
    private void HandleJointBreak(Vector3Int partPos, Direction breakDir)
    {
        int x = partPos.x;
        int y = partPos.y;
        int currentLayer = partPos.z;

        if (activeSpawnedParts == null || x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY) return;

        var partsInCell = activeSpawnedParts[x, y];
        GameObject brokenOffPart = null;

        // 1. USTALAMY, KTO ODPADA
        if (Direction.Backward == breakDir)
        {
            // Pęknięcie w stronę wyższej warstwy (np. Z=1 puszcza Z=2). 
            // Odpada wyższa warstwa.
            int higherLayer = currentLayer + 1;
            if (partsInCell.TryGetValue(higherLayer, out brokenOffPart))
            {
                // Usuwamy ze słownika, bo ten klocek fizycznie opuszcza pojazd!
                partsInCell.Remove(higherLayer);
            }
        }
        else if (Direction.Forward == breakDir)
        {
            // Pęknięcie w stronę niższej warstwy (np. Z=1 odrywa się od ramy Z=0). 
            // Odpadamy MY.
            if (partsInCell.TryGetValue(currentLayer, out brokenOffPart))
            {
                partsInCell.Remove(currentLayer);
            }
        }

        // 2. LOGIKA WYPADANIA KLOCKA Z KRATKI
        if (brokenOffPart != null)
        {
            // Odepnij od pojazdu, żeby jego masa i fizyka działała już osobno

            // KLUCZOWE: Zmiana warstwy fizycznej na złom. 
            // Dzięki temu nie zderzy się z warstwą 0, z którą dzieli pozycję (X, Y)!
            SetLayerRecursively(brokenOffPart, brokenPartLayerID);

            // Dopiero teraz bezpiecznie włączamy twardą kolizję
            Collider2D[] colliders = brokenOffPart.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.isTrigger = false;
            }

            // Wizualny bajer - nadajemy mu lekką, losową siłę odrzutu, żeby "wyskoczył" ze środka auta
            Rigidbody2D rb = brokenOffPart.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(0f, 2f)), ForceMode2D.Impulse);
            }
        }

        RecalculatePartsState();
    }

    // Funkcja pomocnicza, ponieważ zmiana gameObject.layer nie zmienia automatycznie dzieci, 
    // a Twoje Collidery mogą być głębiej w hierarchii prefabu.
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    public void DropPartAtWorldPosition(Vector3 worldPos)
    {
        Vector3Int cellPos = partsTilemap.WorldToCell(worldPos);
        OnSingleClick(cellPos);
        actPartData = null;
    }
    public PartData GrabPartFromGrid(Vector3Int tilePos)
    {
        int x = tilePos.x - offsetX;
        int y = tilePos.y - offsetY;

        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY) return null;

        var cellParts = partDataGrid[x, y].parts;
        if (cellParts.Count == 0) return null;

        int topLayer = -1;
        foreach (int layerId in cellParts.Keys)
        {
            if (layerId > topLayer) topLayer = layerId;
        }

        PartData grabbedPart = cellParts[topLayer].partData;
        RemovePart(x, y);
        return grabbedPart;
    }
}

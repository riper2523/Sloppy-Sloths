#nullable enable
using UnityEngine;
using System.Threading.Tasks;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

using Assets.Prefabs.MapBuilder.MapBuilderManager.States;
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Node.SpecialItems;
using Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector;

using Assets.Prefabs.MapBuilder.Utils;

using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Assets.Prefabs.MapBuilder.Serialization;
using Assets.Prefabs.MapBuilder.Inventory;

[RequireComponent(typeof(IStateProvider))]
[RequireComponent(typeof(MapMover))]
[RequireComponent(typeof(MapScaler))]
public class MapBuilderManager : MonoBehaviour, ISerializableToDTO<IMapStateDTO>
{
    private IStateProvider stateProvider = null!;

    [SerializeField]
    private IMapBuilderManagerState _State = null!;

    private MapMover mapMover = null!;

    private MapScaler mapScaler = null!;

    [SerializeField]
    private bool _movingTheMapRightNow;
    private bool MovingTheMapRightNow
    {
        get => _movingTheMapRightNow;

        set
        {
            _movingTheMapRightNow = value;
            enableMovementButton.CanBeMoved = value;

            if (value)
            {
                State.EscapeWasClicked();
            }
        }
    }

    [SerializeField]
    private EnableMovementButtonBase enableMovementButton = null!;

    [SerializeField]
    private SaveTheMapButtonBase saveTheMapButtonBase = null!;

    [SerializeField]
    private DownloadMapButtonBase downloadMapButtonBase = null!;

    private string lastSavedMapName = "MyMap";

    [SerializeField]
    private MapSavePopup mapSavePopup = null!;

    [SerializeField]
    private MapUploader _mapUploader = null!;

    private IMapBuilderManagerState State
    {
        get => _State;

        set
        {
            _State?.OnDeactivateState();

            _State = value;
            Debug.Assert(value is not null);

            _State.OnActivateState();
        }
    }

    [SerializeField] private MapBuilderItemRegistry itemRegistry = null!;


    [SerializeField] private InventoryInfoManager inventoryManager = null!;
    [SerializeField] private Assets.Prefabs.MapBuilder.StarConfig.StarConfigManager starConfigManager = null!;

    public event System.Action<IMapStateDTO>? OnTestMapRequested;

    private IResizableSpecialItemController? vehicleBuilderInstance;
    private IResizableSpecialItemController? finishLineInstance;
    private List<SpecialItemController> starInstances = new();

    private INodeManager nodeManager = null!;

    [SerializeField]
    private IInputInformation inputInformation = null!;

    void Awake()
    {
        stateProvider = GetComponent<IStateProvider>();
        inputInformation = GetComponentInChildren<IInputInformation>(true);
        mapMover = GetComponent<MapMover>();
        mapScaler = GetComponent<MapScaler>();
        nodeManager = GetComponentInChildren<INodeManager>();

        Debug.Assert(nodeManager is not null);
        Debug.Assert(inputInformation is not null);
        Debug.Assert(_mapUploader is not null);
        Debug.Assert(mapSavePopup is not null);
        Debug.Assert(saveTheMapButtonBase is not null);
        Debug.Assert(mapSavePopup is not null);

        SpecialItemController.SpecialItemSelected += _ => SwitchToBuilderMode();
        PolygonSpecialItemController.PolygonDeleted += () => nodeManager?.DeleteActiveContainer();
        nodeManager!.SelectedContainerChanged += container =>
        {
            if (container != null) SwitchToBuilderMode();
        };
    }

    private void SwitchToBuilderMode()
    {
        var builderState = stateProvider.GetBuilderModeState();
        if (State != builderState)
        {
            State = builderState;
        }
    }

    void Start()
    {
        State = stateProvider.GetBuilderModeState();

        if (MapBuilderTestPreserver.IsTesting && MapBuilderTestPreserver.SavedState != null)
        {
            SetUpUsingDTO(MapBuilderTestPreserver.SavedState, MapBuilderTestPreserver.SavedMapName);
            MapBuilderTestPreserver.IsTesting = false;
        }

        enableMovementButton.ProvidedEvent += _ =>
        {
            MovingTheMapRightNow = !MovingTheMapRightNow;
        };

        saveTheMapButtonBase.ProvidedEvent += _ =>
        {
            mapSavePopup.Show(async (mapName) =>
            {
                var result = await SaveMap(mapName);

                if (result == UploadMapResult.MAP_NAME_TAKEN)
                {
                    mapSavePopup.AskOverwrite(mapName, async () =>
                    {
                        var dto = SerializeToDTO();
                        var updateResult = await _mapUploader.UpdateExistingMap(mapName, dto);
                        mapSavePopup.ShowResult(updateResult, mapName);
                        if (updateResult == UploadMapResult.SUCCESS) lastSavedMapName = mapName;
                    });
                }
                else
                {
                    mapSavePopup.ShowResult(result, mapName);
                    if (result == UploadMapResult.SUCCESS) lastSavedMapName = mapName;
                }
            });
        };

        if (downloadMapButtonBase != null)
        {
            downloadMapButtonBase.ProvidedEvent += _ =>
            {
                DownloadMap();
            };
        }

        Debug.Assert(State is not null);
    }

    private void DownloadMap()
    {
        if (vehicleBuilderInstance == null || finishLineInstance == null)
        {
            Debug.LogError("MapBuilderManager: Cannot download map without VehicleBuilder and FinishLine.");
            if (mapSavePopup != null)
            {
                mapSavePopup.ShowMessage("Cannot download map without Vehicle Builder and Finish Line.", false);
            }
            return;
        }

        var dto = SerializeToDTO();
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
        string downloadsPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
        string safeName = string.Join("_", lastSavedMapName.Split(System.IO.Path.GetInvalidFileNameChars()));
        if (string.IsNullOrWhiteSpace(safeName)) safeName = "DefaultMap";
        string filePath = System.IO.Path.Combine(downloadsPath, safeName + ".map");
        int counter = 1;
        while (System.IO.File.Exists(filePath))
        {
            filePath = System.IO.Path.Combine(downloadsPath, $"{safeName} ({counter}).map");
            counter++;
        }

        System.IO.File.WriteAllText(filePath, json);
        Debug.Log($"Map '{lastSavedMapName}' successfully downloaded to: {filePath}");

        if (mapSavePopup != null)
        {
            mapSavePopup.ShowMessage($"Downloaded to:\n{filePath}", true);
        }
    }

    public async Task<UploadMapResult> SaveMap(string mapName)
    {
        if (_mapUploader == null)
        {
            Debug.LogError("MapBuilderManager: mapUploader is not assigned!");
            return UploadMapResult.MISSING_DRIVER;
        }

        if (nodeManager == null)
        {
            Debug.LogError("MapBuilderManager: NodeManager is not found!");
            return UploadMapResult.UPLOAD_FAILED;
        }

        if (vehicleBuilderInstance == null || finishLineInstance == null)
        {
            Debug.LogError("MapBuilderManager: Cannot save map without VehicleBuilder and FinishLine.");
            return UploadMapResult.MISSING_REQUIRED_ITEMS;
        }

        Debug.Log($"MapBuilderManager: Saving map '{mapName}'...");
        var dto = SerializeToDTO();

        var result = await _mapUploader.UploadMap(mapName, dto);
        UploadMapResultHelper.PrintResult(result, mapName);
        return result;
    }

    public ISpecialItemController? TryAddSpecialItem(SpecialItemController item, Vector3 position)
    {
        if (item.ItemType == MapBuilderItemType.VehicleBuilder)
        {
            if (vehicleBuilderInstance != null)
            {
                Debug.Log("[MapBuilderManager] Vehicle builder already placed. Only one allowed.");
                return null;
            }
        }
        else if (item.ItemType == MapBuilderItemType.FinishLine)
        {
            if (finishLineInstance != null)
            {
                Debug.Log("[MapBuilderManager] Finish line already placed. Only one allowed.");
                return null;
            }
        }

        var instance = Instantiate(item.gameObject, position, Quaternion.identity, transform);
        var controller = instance.GetComponent<SpecialItemController>();

        switch (controller.ItemType)
        {
            case MapBuilderItemType.VehicleBuilder:
                vehicleBuilderInstance = (IResizableSpecialItemController)controller;
                break;
            case MapBuilderItemType.FinishLine:
                finishLineInstance = (IResizableSpecialItemController)controller;
                break;
            case MapBuilderItemType.Star:
                starInstances.Add(controller);
                break;
        }

        instance.SetActive(true);
        SpecialItemController.SpecialItemDeleted += OnSpecialItemDeleted;
        return controller;
    }

    private void OnSpecialItemDeleted(SpecialItemController item)
    {
        switch (item.ItemType)
        {
            case MapBuilderItemType.VehicleBuilder:
                if (item == vehicleBuilderInstance as SpecialItemController) vehicleBuilderInstance = null;
                break;
            case MapBuilderItemType.FinishLine:
                if (item == finishLineInstance as SpecialItemController) finishLineInstance = null;
                break;
            case MapBuilderItemType.Star:
                starInstances.Remove(item);
                break;
        }
    }

    void Update()
    {
        if (saveTheMapButtonBase != null)
        {
            saveTheMapButtonBase.CanBeSaved = (vehicleBuilderInstance != null && finishLineInstance != null);
        }

        if (inputInformation.EscapeKeyWasClicked())
        {
            MovingTheMapRightNow = false;
            State.EscapeWasClicked();
        }

        if (inputInformation.IsCtrlPressed())
        {
            mapScaler.ProcessScaling(Camera.main, inputInformation.ScrollValue());
        }

        var areWeOverAGameObject = inputInformation.AreWeOverAGameObject();

        if (MovingTheMapRightNow && !areWeOverAGameObject)
        {
            mapMover.ProcessPanning(Camera.main,
                    inputInformation.IsPressed(),
                    inputInformation.WeClickedThisFrame(),
                    inputInformation.WeReleasedThisFrame(),
                    Mouse.current.position.ReadValue());
            return;
        }

        if (inputInformation.WeClickedThisFrame())
        {
            if (!areWeOverAGameObject)
            {
                var previousState = State.StateType;
                if (previousState != StateID.BUILDER_MODE)
                {
                    nodeManager?.HandleVoidWasClicked(inputInformation.GetMouseWorldPos());
                }

                if (State.StateType == previousState)
                {
                    State.VoidWasClicked(inputInformation.GetMouseWorldPos());
                }

                Debug.Log($"Void was clicked at {inputInformation.GetMouseWorldPos()}");
            }
            else
            {
                MovingTheMapRightNow = false;

                PointerEventData pointerData = new(EventSystem.current)
                {
                    position = Mouse.current.position.ReadValue()
                };

                List<RaycastResult> results = new();
                EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    Debug.Log($"{results[0].gameObject} was hit");
                }
            }
        }
    }

    public void MoveToBuilderMode()
    {
        if (State.StateType != StateID.BUILDER_MODE)
        {
            State = stateProvider.GetBuilderModeState();
        }
    }

    public void MoveToGearSelectionMode()
    {
        if (State.StateType != StateID.GEAR_SELECT_MODE)
        {
            State = stateProvider.GetGearSelectModeState();
        }
    }

    public void MoveToTestingMode()
    {
        Debug.Log("MoveToTestingMode called. Current state: " + State.StateType);
        if (State.StateType != StateID.TESTING_MODE)
        {
            State = stateProvider.GetTestingModeState();

            if (vehicleBuilderInstance == null || finishLineInstance == null)
            {
                Debug.LogWarning("MapBuilderManager: Map must have a Vehicle Builder and a Finish Line to test!");
                return;
            }

            Debug.Log("MapBuilderManager: Serializing to DTO for testing...");
            Debug.Log("Entering testing mode");
            var dto = SerializeToDTO();
            MapBuilderTestPreserver.SavedState = dto;
            MapBuilderTestPreserver.SavedMapName = lastSavedMapName;
            MapBuilderTestPreserver.IsTesting = true;

            // Generate playable level from DTO
            if (OnTestMapRequested != null)
            {
                Debug.Log("MapBuilderManager: Invoking OnTestMapRequested!");
                OnTestMapRequested.Invoke(dto);
            }
            else
            {
                Debug.LogError("MapBuilderManager: OnTestMapRequested has NO LISTENERS!");
            }
        }
    }

    public void MoveToStarConfigMode()
    {
        if (State.StateType != StateID.STAR_CONFIG_MODE)
        {
            State = stateProvider.GetStarConfigModeState();
        }
    }

    public IMapStateDTO SerializeToDTO()
    {
        var dto = new MapStateDTO
        {
            NodeManager = nodeManager.SerializeToDTO()
        };

        if (inventoryManager != null)
        {
            foreach (SupportedPartType part in System.Enum.GetValues(typeof(SupportedPartType)))
            {
                uint count = inventoryManager.GetPartCount(part);
                if (count > 0)
                {
                    dto.Items[part] = count;
                }
            }
        }

        if (vehicleBuilderInstance != null)
        {
            dto.VehicleBuilder = new VehicleBuilderDTO
            {
                x = vehicleBuilderInstance.gameObject.transform.position.x,
                y = vehicleBuilderInstance.gameObject.transform.position.y,
                z = vehicleBuilderInstance.gameObject.transform.position.z,
                Width = vehicleBuilderInstance.Dimensions.GridWidth,
                Height = vehicleBuilderInstance.Dimensions.GridHeight
            };
        }
        if (finishLineInstance != null)
        {
            dto.FinishLine = new FinishLineDTO
            {
                x = finishLineInstance.gameObject.transform.position.x,
                y = finishLineInstance.gameObject.transform.position.y,
                z = finishLineInstance.gameObject.transform.position.z,
                Width = finishLineInstance.Dimensions.GridWidth,
                Height = finishLineInstance.Dimensions.GridHeight
            };
        }

        foreach (var star in starInstances)
        {
            dto.Stars.Add(new StarDataDTO
            {
                StarGoal = new Assets.Prefabs.LevelSystem.StarManager.StarGoal { goalType = Assets.Prefabs.LevelSystem.StarManager.StarGoalType.CollectStar },
                Position = new PositionDTO(star.transform.position.x, star.transform.position.y, star.transform.position.z)
            });
        }

        if (starConfigManager != null)
        {
            if (starConfigManager.StarForCompletion)
            {
                dto.Stars.Add(new StarDataDTO
                {
                    StarGoal = new Assets.Prefabs.LevelSystem.StarManager.StarGoal { goalType = Assets.Prefabs.LevelSystem.StarManager.StarGoalType.FinishLevel }
                });
            }

            if (starConfigManager.TimeForStar > 0)
            {
                dto.Stars.Add(new StarDataDTO
                {
                    StarGoal = new Assets.Prefabs.LevelSystem.StarManager.StarGoal { goalType = Assets.Prefabs.LevelSystem.StarManager.StarGoalType.TimeLimit, timeLimit = starConfigManager.TimeForStar }
                });
            }
        }

        return dto;
    }

    public void SetUpUsingDTO(IMapStateDTO dto, string? loadedMapName)
    {
        if (!string.IsNullOrEmpty(loadedMapName))
        {
            lastSavedMapName = loadedMapName;
        }

        nodeManager.Clear();
        nodeManager.SetUpUsingDTO(dto.NodeManager);

        // Clear existing special items
        if (vehicleBuilderInstance != null) { Destroy(vehicleBuilderInstance.gameObject); vehicleBuilderInstance = null; }
        if (finishLineInstance != null) { Destroy(finishLineInstance.gameObject); finishLineInstance = null; }
        foreach (var star in starInstances) { if (star != null) Destroy(star.gameObject); }
        starInstances.Clear();

        if (inventoryManager != null)
        {
            foreach (SupportedPartType part in System.Enum.GetValues(typeof(SupportedPartType)))
            {
                inventoryManager.UpdatePartData(new PartDataInfo(part, 0));
            }

            if (dto.Items != null)
            {
                foreach (var kvp in dto.Items)
                {
                    inventoryManager.UpdatePartData(new PartDataInfo(kvp.Key, kvp.Value));
                }
            }
        }

        if (itemRegistry != null)
        {
            if (dto.VehicleBuilder != null)
            {
                var mapping = itemRegistry.GetMapping(MapBuilderItemType.VehicleBuilder);
                TryAddSpecialItem(mapping.Prefab.GetComponent<SpecialItemController>(), new Vector3(dto.VehicleBuilder.x, dto.VehicleBuilder.y, dto.VehicleBuilder.z));
                if (vehicleBuilderInstance != null)
                {
                    vehicleBuilderInstance.Dimensions = (dto.VehicleBuilder.Width, dto.VehicleBuilder.Height);
                }
            }

            if (dto.FinishLine != null)
            {
                var mapping = itemRegistry.GetMapping(MapBuilderItemType.FinishLine);
                TryAddSpecialItem(mapping.Prefab.GetComponent<SpecialItemController>(), new Vector3(dto.FinishLine.x, dto.FinishLine.y, dto.FinishLine.z));
                if (finishLineInstance != null)
                {
                    finishLineInstance.Dimensions = (dto.FinishLine.Width, dto.FinishLine.Height);
                }
            }

            if (dto.Stars != null)
            {
                if (starConfigManager != null)
                {
                    starConfigManager.StarForCompletion = false;
                    starConfigManager.TimeForStar = 0f;
                }

                var mapping = itemRegistry.GetMapping(MapBuilderItemType.Star);
                foreach (var star in dto.Stars)
                {
                    if (star.StarGoal.goalType == Assets.Prefabs.LevelSystem.StarManager.StarGoalType.CollectStar)
                    {
                        TryAddSpecialItem(mapping.Prefab.GetComponent<SpecialItemController>(), new Vector3(star.Position.x, star.Position.y, star.Position.z));
                    }
                    else if (starConfigManager != null)
                    {
                        if (star.StarGoal.goalType == Assets.Prefabs.LevelSystem.StarManager.StarGoalType.FinishLevel)
                        {
                            starConfigManager.StarForCompletion = true;
                        }
                        else if (star.StarGoal.goalType == Assets.Prefabs.LevelSystem.StarManager.StarGoalType.TimeLimit)
                        {
                            starConfigManager.TimeForStar = star.StarGoal.timeLimit;
                        }
                    }
                }
            }
        }
    }

    public void MoveToGameplay()
    {
        nodeManager?.MoveToGameplay();
    }

    public void GoBackToMapSelection()
    {
        SceneManager.LoadScene("MapSelection");
    }

    public void SetUpUsingDTO(IMapStateDTO dto)
    {
        SetUpUsingDTO(dto, null);
    }
}

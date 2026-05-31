#nullable enable
using UnityEngine;
using System.Collections.Generic;
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Serialization;
using Assets.Prefabs.LevelSystem.StarManager;

namespace Assets.Prefabs.LevelSystem.MapLoader
{
    [CreateAssetMenu(fileName = "DTOLevelLoader", menuName = "ScriptableObjects/DTOLevelLoader")]
    public class DTOLevelLoader : ScriptableObject
    {
        [SerializeField] private DTOInstantiator instantiator = null!;
        [SerializeField] private GridAnchor gridAnchorPrefab = null!;
        [SerializeField] private Assets.Prefabs.MapBuilder.Inventory.SupportedPartsRegistry supportedPartsRegistry = null!;

        public LevelData? CreateLevelFromDTO(IMapStateDTO dto)
        {
            // 1. Create a new LevelData instance in memory
            LevelData levelData = CreateInstance<LevelData>();

            // 2. Use DTOInstantiator to build the map hierarchy from the DTO
            // We create a container object to hold the map
            GameObject mapContainer = new("DynamicMap_Source");
            DontDestroyOnLoad(mapContainer);

            // Add GridAnchor at the VehicleBuilder location
            GridAnchor gridAnchor = Instantiate(gridAnchorPrefab, mapContainer.transform);
            gridAnchor.transform.localPosition = new Vector3(dto.VehicleBuilder.x, dto.VehicleBuilder.y, dto.VehicleBuilder.z);

            if (dto.VehicleBuilder != null)
            {
                // gridAnchorGO.transform.localPosition = new Vector3(dto.VehicleBuilder.x, dto.VehicleBuilder.y, dto.VehicleBuilder.z);
                gridAnchor.gridSizeX = (int)dto.VehicleBuilder.Width;
                gridAnchor.gridSizeY = (int)dto.VehicleBuilder.Height;
            }

            // Instantiate NodeManager
            INodeManager? manager = instantiator.InstantiateNodeManager(dto.NodeManager, mapContainer.transform);
            manager?.MoveToGameplay();

            // Instantiate FinishLine using the Instantiator
            if (dto.FinishLine != null)
            {
                instantiator.InstantiateFinishLine(dto.FinishLine, mapContainer.transform);
            }

            // Instantiate Stars using the Instantiator and build star goals
            levelData.starGoals = new List<StarGoal>();
            if (dto.Stars != null)
            {
                foreach (var starDto in dto.Stars)
                {
                    if (starDto.StarGoal.goalType == StarGoalType.CollectStar)
                    {
                        instantiator.InstantiateStar(starDto, mapContainer.transform);
                        levelData.starGoals.Add(starDto.StarGoal);
                    }
                    else
                    {
                        levelData.starGoals.Add(starDto.StarGoal);
                    }
                }
            }

            // Hide it AFTER initialization so Awake() runs on all components
            mapContainer.SetActive(false);

            if (manager == null)
            {
                Debug.LogError("DTOLevelLoader: Failed to instantiate NodeManager from DTO.");
                Destroy(mapContainer);
                return null;
            }

            // The MapLoader expects a prefab it can Instantiate.
            // We provide this GameObject instance.
            levelData.mapPrefab = mapContainer;

            Inventory reconstructedInventory = CreateInstance<Inventory>();
            if (dto.Items != null && supportedPartsRegistry != null)
            {
                foreach (var kvp in dto.Items)
                {
                    var partData = supportedPartsRegistry.GetPartData(kvp.Key);
                    if (partData != null)
                    {
                        reconstructedInventory.itemsMap[partData] = (int)kvp.Value;
                    }
                }
            }

            // 3. Configure gameplay requirements
            levelData.startingItems = reconstructedInventory;

            return levelData;
        }
    }
}

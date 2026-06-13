#nullable enable
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Prefabs.MapBuilder.ServerInteraction;
using Assets.Prefabs.LevelSystem.MapLoader;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace Assets.Prefabs.UI.MapSelection
{
    public class MapSelectionMenu : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private UnityServerDriver serverDriver = null!;
        [SerializeField] private DTOLevelLoader levelLoader = null!;
        [SerializeField] private LevelDataEventChannelSO loadLevelEvent = null!;
        [SerializeField] private string gameplaySceneName = "LevelLoaderScene";

        [Header("UI References")]
        [SerializeField] private RectTransform mapListContainer = null!;
        [SerializeField] private GameObject mapButtonPrefab = null!;
        [SerializeField] private GameObject statusPanel = null!;
        [SerializeField] private TextMeshProUGUI statusText = null!;
        [SerializeField] private Button refreshButton = null!;

        private static GameObject? lastDynamicMapSource;

        private void Awake()
        {
            Debug.Assert(serverDriver != null, "MapSelectionMenu: serverDriver is not assigned!");
            Debug.Assert(levelLoader != null, "MapSelectionMenu: levelLoader is not assigned!");
            Debug.Assert(loadLevelEvent != null, "MapSelectionMenu: loadLevelEvent is not assigned!");
            Debug.Assert(mapListContainer != null, "MapSelectionMenu: mapListContainer is not assigned!");
            Debug.Assert(mapButtonPrefab != null, "MapSelectionMenu: mapButtonPrefab is not assigned!");
            Debug.Assert(statusPanel != null, "MapSelectionMenu: statusPanel is not assigned!");
            Debug.Assert(statusText != null, "MapSelectionMenu: statusText is not assigned!");
            Debug.Assert(refreshButton != null, "MapSelectionMenu: refreshButton is not assigned!");

            // Ensure there is an EventSystem in the scene, otherwise UI won't work
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(async () => await RefreshMapList());
                refreshButton.gameObject.SetActive(false);
            }
        }

        private async void Start()
        {
            await RefreshMapList();
        }

        public async Task RefreshMapList()
        {
            statusPanel?.SetActive(true);
            statusPanel?.transform.SetAsLastSibling();
            refreshButton?.gameObject.SetActive(false);
            statusText?.SetText("Checking server connection...");

            if (serverDriver == null)
            {
                Debug.LogError("MapSelectionMenu: serverDriver is not assigned!");
                statusPanel?.SetActive(false);
                return;
            }

            bool isAlive = await serverDriver.IsServerAliveAsync();
            if (!isAlive)
            {
                statusText?.SetText("Error: Server unreachable.");
                // Keep overlay active so the statusText and refreshButton are visible
                statusPanel?.SetActive(true);
                refreshButton?.gameObject.SetActive(true);
                return;
            }

            statusText?.SetText("Fetching maps from server...");

            // Clear existing buttons
            if (mapListContainer != null)
            {
                foreach (Transform child in mapListContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                Debug.LogWarning("MapSelectionMenu: mapListContainer is not assigned!");
            }

            MapData[]? maps = await serverDriver.GetMapsAsync();

            if (maps == null)
            {
                statusText?.SetText("Error: Failed to fetch maps.");
                statusPanel?.SetActive(true);
                refreshButton?.gameObject.SetActive(true);
                return;
            }

            if (maps.Length == 0)
            {
                statusText?.SetText("No maps found on server.");
                statusPanel?.SetActive(false);
                return;
            }

            statusText?.SetText($"Found {maps.Length} maps.");

            if (mapButtonPrefab == null)
            {
                Debug.LogError("MapSelectionMenu: mapButtonPrefab is not assigned!");
                statusPanel?.SetActive(false);
                return;
            }

            foreach (var map in maps)
            {
                if (mapListContainer == null) break;

                // Play Button
                GameObject playBtnObj = Instantiate(mapButtonPrefab, mapListContainer);
                var playBtn = playBtnObj.GetComponent<Button>();
                var playText = playBtnObj.GetComponentInChildren<TextMeshProUGUI>();
                playText?.SetText($"Play: {map.MapName} (by {map.Owner.Nick})");
                playBtn?.onClick.AddListener(() => OnMapSelected(map.MapName));

                // Edit Button
                GameObject editBtnObj = Instantiate(mapButtonPrefab, mapListContainer);
                var editBtn = editBtnObj.GetComponent<Button>();
                var editText = editBtnObj.GetComponentInChildren<TextMeshProUGUI>();
                editText?.SetText($"Edit: {map.MapName}");
                editBtn?.onClick.AddListener(() => OnMapEditSelected(map.MapName));
            }

            statusPanel?.SetActive(false);
        }

        private async void OnMapEditSelected(string mapName)
        {
            statusPanel?.SetActive(true);
            statusText?.SetText($"Downloading map for editing: {mapName}...");

            if (serverDriver == null) return;
            var (_, mapData) = await serverDriver.GetMapAsync(mapName);

            if (mapData == null || mapData.MapStateDTO == null)
            {
                statusText?.SetText("Failed to download map data.");
                statusPanel?.SetActive(true);
                refreshButton?.gameObject.SetActive(true);
                return;
            }

            statusText?.SetText("Loading MapBuilder...");

            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("MapBuilder");
            sceneLoad.completed += (op) =>
            {
                var builderManager = FindAnyObjectByType<MapBuilderManager>();
                if (builderManager != null)
                {
                    builderManager.SetUpUsingDTO(mapData.MapStateDTO, mapName);
                }
                else
                {
                    Debug.LogError("MapSelectionMenu: Could not find MapBuilderManager in the loaded scene!");
                }
            };
        }

        private async void OnMapSelected(string mapName)
        {
            statusPanel?.SetActive(true);
            statusText?.SetText($"Downloading map: {mapName}...");

            if (serverDriver == null) return;
            var (_, mapData) = await serverDriver.GetMapAsync(mapName);

            if (mapData == null || mapData.MapStateDTO == null)
            {
                statusText?.SetText("Failed to download map data.");
                statusPanel?.SetActive(false);
                return;
            }

            // Convert DTO to LevelData
            LevelData? level = levelLoader.CreateLevelFromDTO(mapData.MapStateDTO);

            if (level == null)
            {
                statusText?.SetText("Failed to process map data.");
                statusPanel?.SetActive(false);
                return;
            }

            // Assign unique ID based on map name so the Save System has a key
            level.uniqueID = mapName;

            // Cleanup previous dynamic map source to avoid memory leaks
            if (lastDynamicMapSource != null)
            {
                Destroy(lastDynamicMapSource);
            }
            lastDynamicMapSource = level.mapPrefab;

            statusText?.SetText("Loading level...");

            // Set the static handover level
            MapLoader.PendingLevel = level;

            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}

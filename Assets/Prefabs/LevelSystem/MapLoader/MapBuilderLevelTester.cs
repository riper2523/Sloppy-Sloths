#nullable enable
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Prefabs.MapBuilder.Serialization;

namespace Assets.Prefabs.LevelSystem.MapLoader
{
    public class MapBuilderLevelTester : MonoBehaviour
    {
        [SerializeField] private DTOLevelLoader levelLoader = null!;
        [SerializeField] private string gameplaySceneName = "LevelLoaderScene";

        private void OnEnable()
        {
            var mbm = FindAnyObjectByType<global::MapBuilderManager>();
            if (mbm != null)
            {
                mbm.OnTestMapRequested += TestMap;
            }
        }

        private void OnDisable()
        {
            var mbm = FindAnyObjectByType<global::MapBuilderManager>();
            if (mbm != null)
            {
                mbm.OnTestMapRequested -= TestMap;
            }
        }

        public void TestMap(IMapStateDTO dto)
        {
            Debug.Log("MapBuilderLevelTester.TestMap called with dto!");
            if (levelLoader == null)
            {
                Debug.LogError("MapBuilderLevelTester: DTOLevelLoader is missing!");
                return;
            }

            var level = levelLoader.CreateLevelFromDTO(dto);
            global::MapLoader.PendingLevel = level;
            
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Utils
{
    public class SceneSwitcher : MonoBehaviour
    {
#if UNITY_EDITOR
        [Tooltip("Drag a Scene Asset here")]
        [SerializeField] private UnityEditor.SceneAsset sceneAsset = null!;
#endif
        [SerializeField, HideInInspector] private string sceneName = "";

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (sceneAsset != null)
            {
                sceneName = sceneAsset.name;
            }
            else
            {
                sceneName = "";
            }
#endif
        }

        public void LoadScene()
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("SceneSwitcher: No scene specified to load!");
            }
        }
    }
}

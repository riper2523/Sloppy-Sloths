using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Prefabs.UI.MapSelection;

namespace Assets.Prefabs.UI.MapSelection
{
    public class MapSelectionBootstrap : MonoBehaviour
    {
        [SerializeField] private MapSelectionMenu menuController;

        void Awake()
        {
            if (menuController == null)
            {
                menuController = Object.FindAnyObjectByType<MapSelectionMenu>();
            }

            if (menuController == null)
            {
                Debug.Log("MapSelectionBootstrap: Creating UI at runtime...");
                CreateDefaultUI();
            }
        }

        private void CreateDefaultUI()
        {
            GameObject canvasObj = new GameObject("MapSelectionCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("BackgroundPanel");
            panel.transform.SetParent(canvasObj.transform, false);
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "SELECT A MAP";
            title.fontSize = 48;
            title.alignment = TextAlignmentOptions.Center;
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1f);
            titleRect.sizeDelta = Vector2.zero;

            // ... more runtime UI creation could go here, 
            // but it's better if the user just attaches the MapSelectionMenu prefab.

            Debug.LogWarning("MapSelectionBootstrap: Minimal UI created. Please assign a proper UI prefab to MapSelectionMenu for full functionality.");
        }
    }
}

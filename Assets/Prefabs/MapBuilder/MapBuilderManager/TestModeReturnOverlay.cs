#nullable enable
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager
{
    public class TestModeReturnOverlay : MonoBehaviour
    {
        private static TestModeReturnOverlay? instance;

        public static void CheckAndSpawnOverlay()
        {
            if (MapBuilderTestPreserver.IsTesting && instance == null)
            {
                var go = new GameObject("TestModeReturnOverlay");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<TestModeReturnOverlay>();
                instance.CreateUI();
            }
        }

        private void CreateUI()
        {
            // Create Canvas
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Ensure it renders on top of everything

            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();

            // Create Button
            var buttonObj = new GameObject("ReturnButton");
            buttonObj.transform.SetParent(transform, false);
            var rect = buttonObj.AddComponent<RectTransform>();

            // Anchor top right
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20);
            rect.sizeDelta = new Vector2(200, 60);

            var img = buttonObj.AddComponent<Image>();
            img.color = new Color(0.9f, 0.1f, 0.2f, 0.9f); // Red button

            var btn = buttonObj.AddComponent<Button>();
            btn.onClick.AddListener(OnReturnClicked);

            // Create Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "Return to Builder";
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = 20;
            tmpText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private void OnReturnClicked()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                instance = null;
            }
            SceneManager.LoadScene("MapBuilder");
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}

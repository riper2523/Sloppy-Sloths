using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager
{
    public class MapSavePopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button confirmSaveButton;
        [SerializeField] private Button cancelButton;

        [Header("Result UI")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button closeResultButton;

        [Header("Confirmation UI")]
        [SerializeField] private GameObject confirmOverwritePanel;
        [SerializeField] private TextMeshProUGUI confirmOverwriteText;
        [SerializeField] private Button overwriteYesButton;
        [SerializeField] private Button overwriteNoButton;

        private Action<string> onSaveConfirmed;
        private Action onOverwriteConfirmed;

        void Awake()
        {
            popupPanel?.SetActive(false);
            resultPanel?.SetActive(false);
            confirmOverwritePanel?.SetActive(false);

            confirmSaveButton.onClick.AddListener(OnConfirmClick);
            cancelButton.onClick.AddListener(Hide);
            closeResultButton.onClick.AddListener(() => resultPanel.SetActive(false));

            overwriteYesButton?.onClick.AddListener(() =>
            {
                confirmOverwritePanel?.SetActive(false);
                onOverwriteConfirmed?.Invoke();
            });
            overwriteNoButton?.onClick.AddListener(() => confirmOverwritePanel?.SetActive(false));
        }

        public void Show(Action<string> saveCallback)
        {
            onSaveConfirmed = saveCallback;
            popupPanel?.SetActive(true);
            resultPanel?.SetActive(false);
            confirmOverwritePanel?.SetActive(false);
            if (nameInputField != null)
            {
                nameInputField.text = "NewMap_" + DateTime.Now.ToString("yyyyMMdd_HHmm");
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }
        }

        public void Hide()
        {
            popupPanel?.SetActive(false);
            confirmOverwritePanel?.SetActive(false);
        }

        private void OnConfirmClick()
        {
            string mapName = nameInputField != null ? nameInputField.text : "UnnamedMap";
            if (string.IsNullOrWhiteSpace(mapName)) return;

            Hide();
            onSaveConfirmed?.Invoke(mapName);
        }

        public void ShowResult(UploadMapResult result, string mapName)
        {
            resultPanel?.SetActive(true);
            if (resultText != null)
            {
                resultText.text = UploadMapResultHelper.GetMessage(result, mapName);

                // Color coding for success/failure
                bool isSuccess = result == UploadMapResult.SUCCESS || result == UploadMapResult.MAP_UPDATED;
                resultText.color = isSuccess ? Color.green : Color.red;
            }
        }

        public void AskOverwrite(string mapName, Action onConfirmed)
        {
            onOverwriteConfirmed = onConfirmed;
            if (confirmOverwritePanel != null)
            {
                confirmOverwritePanel.SetActive(true);
                if (confirmOverwriteText != null)
                    confirmOverwriteText.text = $"Map '{mapName}' already exists. Overwrite?";
            }
            else
            {
                // Fallback if panel not assigned
                onConfirmed?.Invoke();
            }
        }
    }
}

#nullable enable
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Prefabs.MapBuilder.StarConfig
{
    public class StarConfigUIController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private StarConfigManager starConfigManager = null!;

        [Header("UI Elements")]
        [SerializeField] private Toggle completionToggle = null!;
        [SerializeField] private TMP_InputField timeInputField = null!;

        void Awake()
        {
            Debug.Assert(starConfigManager != null, "StarConfigUIController: starConfigManager is not assigned!");
            Debug.Assert(completionToggle != null, "StarConfigUIController: completionToggle is not assigned!");
            Debug.Assert(timeInputField != null, "StarConfigUIController: timeInputField is not assigned!");

            if (completionToggle != null)
            {
                completionToggle.onValueChanged.AddListener(OnCompletionToggleChanged);
            }

            if (timeInputField != null)
            {
                timeInputField.onValueChanged.AddListener(OnTimeInputChanged);
            }
        }

        void OnEnable()
        {
            if (starConfigManager != null)
            {
                if (completionToggle != null)
                    completionToggle.isOn = starConfigManager.StarForCompletion;

                if (timeInputField != null)
                {
                    if (starConfigManager.TimeForStar > 0)
                        timeInputField.text = starConfigManager.TimeForStar.ToString("F1");
                    else
                        timeInputField.text = "";
                }
            }
        }

        private void OnCompletionToggleChanged(bool isOn)
        {
            if (starConfigManager != null)
            {
                starConfigManager.StarForCompletion = isOn;
            }
        }

        private void OnTimeInputChanged(string value)
        {
            if (starConfigManager != null)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    starConfigManager.TimeForStar = 0f;
                }
                else if (float.TryParse(value, out float parsedTime))
                {
                    starConfigManager.TimeForStar = parsedTime;
                }
            }
        }
    }
}

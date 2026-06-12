using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Prefabs.MapBuilder.Buttons
{
    public interface IValidator
    {
        bool IsValid(string text);
    }

    abstract class ButtonTextFieldGroup : MonoBehaviour
    {
        protected Button Button;
        protected TMP_InputField InputField;
        protected IValidator Validator { get; set; }

        private string lastValidValue = "";

        protected void Awake()
        {
            Button = GetComponentInChildren<Button>();
            InputField = GetComponentInChildren<TMP_InputField>();

            Debug.Assert(Button is not null);
            Debug.Assert(InputField is not null);

            InputField.onValueChanged.AddListener(HandleValueChanged);
        }

        private void HandleValueChanged(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                lastValidValue = newValue;
                return;
            }

            if (Validator is not null && Validator.IsValid(newValue))
            {
                lastValidValue = newValue;
            }
            else if (Validator is not null)
            {
                InputField.text = lastValidValue;
            }
        }

        abstract public void DispatchTheEvent();
    }
}

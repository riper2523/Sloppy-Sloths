using System;
using UnityEngine;
using Assets.Prefabs.MapBuilder.Utils;
using Assets.Prefabs.MapBuilder.MapBuilderManager;
using Assets.Prefabs.MapBuilder.Buttons;

class RotationValueValidator : IValidator
{
    public bool IsValid(string text)
    {
        // Allow starting a negative number or a decimal
        if (text == "-" || text == "." || text == "-.") return true;

        return float.TryParse(text, out _);
    }
}

namespace Assets.Prefabs.MapBuilder.Buttons
{
    class RotationManager : ButtonTextFieldGroup, IEventProvider<Rotation>
    {
        public event Action<Rotation> ProvidedEvent;

        new void Awake()
        {
            base.Awake();
            Validator = new RotationValueValidator();
        }

        override public void DispatchTheEvent()
        {
            if (string.IsNullOrEmpty(InputField.text))
            {
                Debug.Log("No value provided");
                return;
            }

            if (float.TryParse(InputField.text, out float rotationValue))
            {
                var rotationTransform = new Rotation(rotationValue);
                ProvidedEvent.Invoke(rotationTransform);
            }
            else
            {
                Debug.LogError("The provided value is invalid, this shouldn't happen");
            }
        }
    }
}

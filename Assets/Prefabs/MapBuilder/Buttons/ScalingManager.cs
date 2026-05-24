using System;
using UnityEngine;
using Assets.Prefabs.MapBuilder.Utils;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

using Assets.Prefabs.MapBuilder.Buttons;

class ScaleValueValidator : IValidator
{
    public bool IsValid(string text)
    {
        // Allow starting a decimal number
        if (text == ".") return true;

        if (float.TryParse(text, out float value))
        {
            return value > 0;
        }
        return false;
    }
}

namespace Assets.Prefabs.MapBuilder.Buttons
{
    class ScalingManager : ButtonTextFieldGroup, IEventProvider<Scaling>
    {
        public event Action<Scaling> ProvidedEvent;

        new void Awake()
        {
            base.Awake();
            Validator = new ScaleValueValidator();
        }

        override public void DispatchTheEvent()
        {
            if (string.IsNullOrEmpty(InputField.text))
            {
                Debug.Log("No value provided");
                return;
            }

            if (float.TryParse(InputField.text, out float scalingValue))
            {
                var rotationTransform = new Scaling(scalingValue);
                ProvidedEvent?.Invoke(rotationTransform);
            }
            else
            {
                Debug.LogError("The provided value is invalid, this shouldn't happen");
            }
        }
    }
}

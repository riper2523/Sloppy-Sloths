using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Prefabs.MapBuilder.Utils;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

namespace Assets.Prefabs.MapBuilder.Buttons
{
    public class RotationManager : MonoBehaviour, IEventProvider<Rotation>
    {
        public event Action<Rotation> ProvidedEvent;
        private Slider _slider;
        private float _lastValue;

        void Awake()
        {
            _slider = GetComponentInChildren<Slider>();
            if (_slider != null)
            {
                _lastValue = _slider.value;
                _slider.onValueChanged.AddListener(HandleValueChanged);
            }
            else
            {
                Debug.LogWarning("RotationManager: No Slider component found in children.");
            }
        }

        private void HandleValueChanged(float value)
        {
            float delta = value - _lastValue;
            _lastValue = value;
            if (Mathf.Abs(delta) > 0.001f)
            {
                ProvidedEvent?.Invoke(new Rotation(delta));
            }
        }

        private IInputInformation _inputInformation;

        void Update()
        {
            if (_inputInformation == null)
            {
                _inputInformation = FindAnyObjectByType<InputInformation>();
            }

            if (_slider != null && _inputInformation != null && _inputInformation.WeReleasedThisFrame())
            {
                _slider.SetValueWithoutNotify(0f);
                _lastValue = 0f;
            }
        }
    }
}

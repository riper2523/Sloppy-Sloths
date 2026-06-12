using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Prefabs.MapBuilder.Utils;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

namespace Assets.Prefabs.MapBuilder.Buttons
{
    public class ScalingManager : MonoBehaviour, IEventProvider<Scaling>
    {
        public event Action<Scaling> ProvidedEvent;
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
                Debug.LogWarning("ScalingManager: No Slider component found in children.");
            }
        }

        private void HandleValueChanged(float value)
        {
            if (value <= 0.01f) value = 0.01f;
            if (_lastValue <= 0.01f) _lastValue = 0.01f;

            float delta = value / _lastValue;
            _lastValue = value;

            if (Mathf.Abs(delta - 1f) > 0.001f)
            {
                ProvidedEvent?.Invoke(new Scaling(delta));
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
                _slider.SetValueWithoutNotify(1f);
                _lastValue = 1f;
            }
        }
    }
}

using UnityEngine;
using TMPro;
using Assets.Prefabs.MapBuilder.Node.SpecialItems;

namespace Assets.Prefabs.MapBuilder.Popups
{
    public class SpecialResizableConfigUI : SpecialItemConfigUI
    {
        public TMP_InputField widthInput;
        public TMP_InputField heightInput;

        //Dont change dimensions after chan
        private void DimUpdater(string _)
        {
            UpdateDimensions();
        }

        private IResizableSpecialItemController _currentResizableItem;

        private void SetListeners()
        {
            widthInput.onValueChanged.AddListener(DimUpdater);
            heightInput.onValueChanged.AddListener(DimUpdater);
        }

        public IResizableSpecialItemController CurrentResizableItem
        {
            get => _currentResizableItem;
            set
            {
                if (_currentResizableItem == value)
                {
                    return;
                }

                _currentResizableItem = value;

                if (_currentResizableItem is not null)
                {
                    Debug.Log($"Setting resizable item to {_currentResizableItem} {CurrentResizableItem.Dimensions}");

                    widthInput.onValueChanged.RemoveListener(DimUpdater);
                    heightInput.onValueChanged.RemoveListener(DimUpdater);

                    widthInput.text = CurrentResizableItem.Dimensions.GridWidth.ToString();
                    heightInput.text = CurrentResizableItem.Dimensions.GridHeight.ToString();

                    SetListeners();
                }
            }
        }

        private TMP_InputField FindInputFieldByName(string name)
        {
            var inputFields = GetComponentsInChildren<TMP_InputField>(true);
            foreach (var field in inputFields)
            {
                if (field.gameObject.name == name)
                {
                    return field;
                }
            }
            return null;
        }

        public void Awake()
        {
            Debug.Assert(widthInput is not null);
            Debug.Assert(heightInput is not null);

            widthInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            heightInput.contentType = TMP_InputField.ContentType.IntegerNumber;

            SetListeners();
        }

        public override void Trigger(ISpecialItemController specialItemController)
        {
            base.Trigger(specialItemController);

            var resizableSpecialItemController = specialItemController as IResizableSpecialItemController;
            CurrentResizableItem = resizableSpecialItemController;
        }

        public void UpdateDimensions()
        {
            if (CurrentResizableItem == null) return;

            // If the value in the box is invalid then we will not be changing that Dimension
            if (!uint.TryParse(widthInput.text, out uint newWidth) || newWidth <= 0)
            {
                newWidth = CurrentResizableItem.Dimensions.GridWidth;
                widthInput.text = newWidth.ToString();
            }

            if (!uint.TryParse(heightInput.text, out uint newHeight) && newHeight <= 0)
            {
                newHeight = CurrentResizableItem.Dimensions.GridHeight;
                heightInput.text = newHeight.ToString();
            }

            Debug.Log($"Clicked {CurrentResizableItem} setting dimensions {newWidth} {newHeight}");
            CurrentResizableItem.Dimensions = (newWidth, newHeight);
        }
    }
}

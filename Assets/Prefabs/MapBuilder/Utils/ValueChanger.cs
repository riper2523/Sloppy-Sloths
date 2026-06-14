using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ValueChanger : MonoBehaviour
{
    [SerializeField]
    private float Value = 0;

    [SerializeField]
    private TMP_InputField InputField;

    [Header("Constraints")]
    [SerializeField]
    private bool useLimits = false;

    [SerializeField]
    private float minValue = 1f;

    [SerializeField]
    private float maxValue = 100f;

    private void Awake()
    {
        Debug.Assert(InputField is not null);
    }

    public void ChangeValue()
    {
        // Attempt to parse the current text as a float
        if (float.TryParse(InputField.text, out float currentValue))
        {
            currentValue += Value;

            if (useLimits)
            {
                currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
            }

            InputField.text = currentValue.ToString();
        }
        else
        {
            float fallbackValue = useLimits ? Mathf.Clamp(Value, minValue, maxValue) : Value;
            InputField.text = fallbackValue.ToString();
        }
    }
}

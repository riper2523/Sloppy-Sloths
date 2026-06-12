#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Prefabs.MapBuilder.Inventory;

/// <summary>
/// Component on each instantiated inventory row button.
/// Shows the part icon, an integer input field and a "Set" button.
/// Pressing the button pushes the entered amount into InventoryInfoManager.
/// </summary>
public class PartButton : MonoBehaviour
{
    [SerializeField] private Image partImage = null!;
    [SerializeField] private TMP_InputField amountInput = null!;


    public SupportedPartType Part { get; private set; }

    private InventoryInfoManager? inventoryInfoManager;

    private void Awake()
    {
        Debug.Assert(partImage != null, "PartButton: partImage is not assigned!");
        Debug.Assert(amountInput != null, "PartButton: amountInput is not assigned!");
    }

    public void Initialize(SupportedPartType part, Sprite icon, InventoryInfoManager manager)
    {
        Part = part;
        inventoryInfoManager = manager;
        partImage.sprite = icon;
        amountInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        amountInput.text = inventoryInfoManager.GetPartCount(part).ToString();
        amountInput.onValueChanged.AddListener(OnInputChanged);
    }

    private void OnInputChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            inventoryInfoManager?.UpdatePartData(new PartDataInfo(Part, 0));
        }
        else if (uint.TryParse(value, out uint amount))
        {
            inventoryInfoManager?.UpdatePartData(new PartDataInfo(Part, amount));
        }
    }
}

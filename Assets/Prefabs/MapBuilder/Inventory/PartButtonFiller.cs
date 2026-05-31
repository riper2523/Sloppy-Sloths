#nullable enable
using System;
using UnityEngine;
using Assets.Prefabs.MapBuilder.Inventory;
#if UNITY_EDITOR
using UnityEditor;
#endif

class PartButtonFiller : MonoBehaviour
{
    [SerializeField] private InventoryInfoManager inventoryInfoManager = null!;
    [SerializeField] private SupportedPartsRegistry supportedPartsRegistry = null!;
    [SerializeField] private GameObject partRelatedButtonPrefab = null!;

    void Awake()
    {
        PopulateButtons();
    }

    [ContextMenu("Populate Buttons in Editor")]
    public void PopulateButtonsInEditor()
    {
        PopulateButtons();
    }

    [ContextMenu("Clear Buttons")]
    public void ClearButtons()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }
    }

    private void PopulateButtons()
    {
        if (inventoryInfoManager == null || supportedPartsRegistry == null || partRelatedButtonPrefab == null)
        {
            Debug.LogError("PartButtonFiller: Missing references!");
            return;
        }

        ClearButtons();

        foreach (SupportedPartType part in Enum.GetValues(typeof(SupportedPartType)))
        {
            if (!supportedPartsRegistry.TryGetPartData(part, out PartData? partData) || partData == null)
                continue;

            GameObject buttonGO;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                buttonGO = (GameObject)PrefabUtility.InstantiatePrefab(partRelatedButtonPrefab, transform);
            }
            else
            {
                buttonGO = Instantiate(partRelatedButtonPrefab, transform);
            }
#else
            buttonGO = Instantiate(partRelatedButtonPrefab, transform);
#endif

            if (buttonGO == null) continue;

            var partButton = buttonGO.GetComponent<PartButton>();
            if (partButton == null)
            {
                Debug.LogError("PartButtonFiller: partRelatedButtonPrefab is missing a PartButton component!");
                continue;
            }

            partButton.Initialize(part, partData.partSpriteUI, inventoryInfoManager);
        }
    }
}

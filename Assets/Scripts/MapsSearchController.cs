using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapsSearchController : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField searchInput;

    [SerializeField]
    private RectTransform resultsViewport;

    [SerializeField]
    private RectTransform resultsContent;

    [SerializeField]
    private ScrollRect resultsScrollRect;

    [SerializeField]
    private Color itemBackgroundColor = new(0.1f, 0.1f, 0.1f, 0.6f);

    private readonly List<string> mapEntries = new()
    {
        "Mountain Pass",
        "Old Quarry",
        "Pine Valley",
        "Sunset Dunes",
        "Frozen Lake",
        "Copper Canyon",
        "Misty Forest",
        "Dry Riverbed",
        "Sloth Speedway",
        "Timber Hills",
        "Crystal Ridge",
        "Rusted Bridge",
    };

    private readonly List<GameObject> spawnedItems = new();

    private void Awake()
    {
        if (searchInput == null || resultsViewport == null || resultsContent == null || resultsScrollRect == null)
        {
            Debug.LogError("MapsSearchController is missing UI references.");
            enabled = false;
            return;
        }

        searchInput.onValueChanged.AddListener(RefreshResults);
        RefreshResults(searchInput.text);
    }

    private readonly string mainMenuSceneName = "MainMenu";

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            GoBack();
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        searchInput?.onValueChanged.RemoveListener(RefreshResults);
    }

    private void RefreshResults(string query)
    {
        ClearResults();

        foreach (var entry in mapEntries)
        {
            if (!string.IsNullOrWhiteSpace(query) && entry.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            spawnedItems.Add(CreateResultItem(entry));
        }

        resultsScrollRect.verticalNormalizedPosition = 1f;
    }

    private GameObject CreateResultItem(string label)
    {
        var rowObject = new GameObject($"{label} Item", typeof(RectTransform), typeof(LayoutElement), typeof(Image));
        var rowRect = rowObject.GetComponent<RectTransform>();
        rowRect.SetParent(resultsContent, false);
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.sizeDelta = new Vector2(0f, 34f);

        var layoutElement = rowObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 34f;

        var background = rowObject.GetComponent<Image>();
        background.color = itemBackgroundColor;
        background.raycastTarget = false;

        var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(rowRect, false);
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);

        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = 20f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.text = label;
        text.raycastTarget = false;

        return rowObject;
    }

    private void ClearResults()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }

        spawnedItems.Clear();
    }
}

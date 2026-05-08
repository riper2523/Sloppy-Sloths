using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Listening To")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;
    [SerializeField] GameObject partButtonPrefab;
    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject removePartButtonPrefab;
    [SerializeField] Inventory inventory;
    [SerializeField] GridManager gridManager;
    private Dictionary<PartData, PartButtonScript> itemsMap = new Dictionary<PartData, PartButtonScript>();

    private void OnEnable() => loadLevelEvent.OnEventRaised += InitializeLevel;
    private void OnDisable() => loadLevelEvent.OnEventRaised -= InitializeLevel;

    public void InitializeLevel(LevelData levelData)
    {
        foreach (Transform child in buttonParent.transform) 
        {
            Destroy(child.gameObject);
        }
        itemsMap.Clear();
        inventory = levelData.startingItems;
        CreateRemovePartButton();
        foreach (var entry in inventory.itemsMap) 
        {
            CreateButtonForPart(entry.Key, entry.Value);
        }
    }
    public bool TryUsePart(PartData part)
    {
        if (inventory.itemsMap.TryGetValue(part, out int count) && count > 0)
        {
            inventory.itemsMap[part] = count - 1;
            if (itemsMap.TryGetValue(part, out PartButtonScript button))
            {
                button.SetPartCount(count - 1);
            }
            return true;
        }
        return false;
    }
    public bool AddPart(PartData part, int amount)
    {
        if (inventory.itemsMap.ContainsKey(part))
        {
            inventory.itemsMap[part] += amount;
            if (itemsMap.TryGetValue(part, out PartButtonScript button))
            {
                button.SetPartCount(inventory.itemsMap[part]);
            }
            return true;
        }
        else
        {
            inventory.itemsMap.Add(part, amount);
            CreateButtonForPart(part, amount);
            return true;
        }
    }
    private void CreateButtonForPart(PartData part, int count)
    {
        GameObject buttonObj = Instantiate(partButtonPrefab, buttonParent.transform);
        PartButtonScript button = buttonObj.GetComponent<PartButtonScript>();
        button.Initialize(part, gridManager);
        button.SetPartCount(count);
        itemsMap.Add(part, button);
    }
    private void CreateRemovePartButton()
    {
        GameObject buttonObj = Instantiate(removePartButtonPrefab, buttonParent.transform);
        RemovePartButtonScript button = buttonObj.GetComponent<RemovePartButtonScript>();
        button.Initialize(gridManager);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject partButtonPrefab;
    [SerializeField] private GameObject buttonParent;
    [SerializeField] private Inventory inventory;
    [SerializeField] private GridManager gridManager;
    
    private Dictionary<PartData, PartButtonScript> itemsMap = new Dictionary<PartData, PartButtonScript>();

    public void InitializeLevel(List<InventoryEntry> startingItems)
    {
        foreach (Transform child in buttonParent.transform)
        {
            Destroy(child.gameObject);
        }
        
        itemsMap.Clear();
        inventory.itemsMap.Clear();

        foreach (var entry in startingItems)
        {
            AddPart(entry.part, entry.amount);
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
}

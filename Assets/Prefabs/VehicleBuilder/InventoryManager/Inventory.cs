using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "Scriptable Objects/Inventory")]
public class Inventory : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] private List<InventoryEntry> initialItemsList = new List<InventoryEntry>();
    public Dictionary<PartData, int> itemsMap = new Dictionary<PartData, int>();
    public void OnAfterDeserialize()
    {
        itemsMap.Clear();
        foreach (var entry in initialItemsList)
        {
            if (entry.part != null && !itemsMap.ContainsKey(entry.part))
            {
                itemsMap.Add(entry.part, entry.amount);
            }
        }
    }

    public void ResetInventory()
    {
        itemsMap.Clear();
        foreach (var entry in initialItemsList)
        {
            if (entry.part != null && !itemsMap.ContainsKey(entry.part))
            {
                itemsMap.Add(entry.part, entry.amount);
            }
        }
    }

    public void OnBeforeSerialize()
    {
    }
}

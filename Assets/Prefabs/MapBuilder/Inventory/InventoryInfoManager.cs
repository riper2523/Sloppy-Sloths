#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Inventory
{
    using InventoryConstraints = ReadOnlyDictionary<SupportedPartType, uint>;

    public record PartDataInfo(SupportedPartType PartKind, uint Amount);

    [CreateAssetMenu(fileName = "InventoryInfoManager", menuName = "Scriptable Objects/InventoryInfoManager")]
    public class InventoryInfoManager : ScriptableObject
    {
        [SerializeField] private SupportedPartsRegistry supportedPartsRegistry = null!;

        private Dictionary<SupportedPartType, uint> mapping = new();
        private ReadOnlyDictionary<SupportedPartType, uint> Mapping { get => new ReadOnlyDictionary<SupportedPartType, uint>(mapping); }

        private void OnEnable()
        {
            Debug.Assert(supportedPartsRegistry != null, "InventoryInfoManager: supportedPartsRegistry is not assigned!");
            mapping = new();
        }

        public void UpdatePartData(PartDataInfo part)
        {
            mapping[part.PartKind] = part.Amount;
        }

        public uint GetPartCount(SupportedPartType partKind)
        {
            if (mapping.TryGetValue(partKind, out uint amount))
                return amount;
            return 0;
        }

        public global::Inventory GetInventory()
        {
            var inventory = CreateInstance<global::Inventory>();
            foreach (var (part, amount) in mapping)
            {
                var partData = supportedPartsRegistry.GetPartData(part);
                if (partData != null)
                    inventory.itemsMap[partData] = (int)amount;
            }
            return inventory;
        }
    }
}

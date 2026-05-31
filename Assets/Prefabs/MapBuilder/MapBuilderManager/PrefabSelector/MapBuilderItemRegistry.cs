using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Prefabs.MapBuilder.Popups;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector
{
    [System.Serializable]
    public struct MapBuilderItemMapping
    {
        public MapBuilderItemType ItemType;
        public GameObject Prefab;
        public SpecialItemConfigUI ConfigPanel;
    }

    public class MapBuilderItemRegistry : MonoBehaviour
    {
        public List<MapBuilderItemMapping> Mappings = new();

        public MapBuilderItemMapping GetMapping(MapBuilderItemType type)
        {
            int index = Mappings.FindIndex(m => m.ItemType == type);

            Debug.Assert(index >= 0, $"No value set for {type}");
            if (index >= 0)
                return Mappings[index];
            return default;
        }


        private void Awake()
        {
            if (Mappings == null) return;
            foreach (var mapping in Mappings)
            {
                if (mapping.ItemType != MapBuilderItemType.Unset)
                {
                    if (mapping.ItemType != MapBuilderItemType.Polygon)
                    {
                        Debug.Assert(mapping.ConfigPanel != null, $"Missing ConfigPanel reference for MapBuilderItemType.{mapping.ItemType} in MapBuilderItemRegistry!");
                    }

                    Debug.Assert(mapping.Prefab != null, $"Missing Prefab reference for MapBuilderItemType.{mapping.ItemType} in MapBuilderItemRegistry!");
                }
            }
        }

        private void OnValidate()
        {
            if (Mappings == null) return;

            var seenTypes = new HashSet<MapBuilderItemType>();

            for (int i = 0; i < Mappings.Count; i++)
            {
                var mapping = Mappings[i];
                if (mapping.ItemType == MapBuilderItemType.Unset)
                    continue;

                if (!seenTypes.Add(mapping.ItemType))
                {
                    Debug.LogWarning($"[MapBuilderItemRegistry] Duplicate MapBuilderItemType '{mapping.ItemType}' detected. Setting to Unset.");
                    mapping.ItemType = MapBuilderItemType.Unset;
                    Mappings[i] = mapping;
                }
            }
        }
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Inventory
{
    public enum SupportedPartType
    {
        SlothPD,
        WoodSquarePD,
        MotorWheelPD,
        MetalSquarePD,
        MetalWheelPD,
        TinyWheelPD,
        WoodWheelPD
    }

    [CreateAssetMenu(fileName = "SupportedPartsRegistry", menuName = "Scriptable Objects/SupportedPartsRegistry")]
    public class SupportedPartsRegistry : ScriptableObject
    {
        [Serializable]
        public struct Mapping
        {
            public SupportedPartType part;
            public PartData data;
        }

        [SerializeField] private List<Mapping> mappings = new();

        public PartData? GetPartData(SupportedPartType part)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.part == part)
                    return mapping.data;
            }

            Debug.LogWarning($"SupportedPartsRegistry: No PartData mapped for {part}");
            return null;
        }

        public bool TryGetPartData(SupportedPartType part, out PartData? data)
        {
            data = GetPartData(part);
            return data != null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (SupportedPartType part in Enum.GetValues(typeof(SupportedPartType)))
            {
                bool found = false;
                foreach (var mapping in mappings)
                {
                    if (mapping.part == part)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    mappings.Add(new Mapping { part = part });
                }
            }
        }
#endif
    }
}

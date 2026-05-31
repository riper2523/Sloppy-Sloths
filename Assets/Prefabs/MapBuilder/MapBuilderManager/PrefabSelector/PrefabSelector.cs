#nullable enable
using UnityEngine;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector
{
    public delegate void PrefabSelectedHandler(MapBuilderItemType type);

    public class PrefabSelector : MonoBehaviour
    {
        public PrefabSelectedHandler? PrefabSelected;

        public void TriggerSelectPrefab(MapBuilderItemType type)
        {
            PrefabSelected?.Invoke(type);
        }
    }
}

#nullable enable
using UnityEngine;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector
{
    public delegate void PrefabSelectedHandler(GameObject prefab);

    public class PrefabSelector : MonoBehaviour
    {
        public PrefabSelectedHandler? PrefabSelected;

        public void TriggerSelectPrefab(GameObject prefab)
        {
            PrefabSelected?.Invoke(prefab);
        }
    }
}

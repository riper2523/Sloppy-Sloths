using UnityEngine;
using UnityEngine.EventSystems;

using Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector;

public class PrefabSelectButton : MonoBehaviour
{
    [SerializeField]
    public MapBuilderItemType ItemType;

    private PrefabSelector prefabSelector;

    void InitPrefabAndEnableIt(MapBuilderItemType type)
    {
        ItemType = type;
        gameObject.SetActive(true);
    }

    void Awake()
    {
        prefabSelector = GetComponentInParent<PrefabSelector>();
        Debug.Assert(prefabSelector is not null);
    }

    public void SelectPrefab()
    {
        Debug.Log($"Prefab type {ItemType} was selected {prefabSelector}");
        prefabSelector.TriggerSelectPrefab(ItemType);
    }
}

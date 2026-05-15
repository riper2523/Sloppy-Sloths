using UnityEngine;
using UnityEngine.EventSystems;

using Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector;

public class PrefabSelectButton : MonoBehaviour
{
    [SerializeField]
    public GameObject Prefab;

    [SerializeField]
    private PrefabSelector prefabSelector;

    void InitPrefabAndEnableIt(GameObject prefab)
    {
        Prefab = prefab;
        gameObject.SetActive(true);
    }

    void Awake()
    {
        prefabSelector = GetComponentInParent<PrefabSelector>();
        Debug.Assert(prefabSelector is not null);
        Debug.Assert(Prefab is not null);

        prefabSelector.TriggerSelectPrefab(Prefab);
    }

    public void SelectPrefab()
    {
        Debug.Log($"Prefab {Prefab} was selected {prefabSelector}");
        prefabSelector.TriggerSelectPrefab(Prefab);
    }
}

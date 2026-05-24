using UnityEngine;
using UnityEngine.EventSystems;

using Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector;

public class PrefabSelectButton : MonoBehaviour
{
    [SerializeField]
    public GameObject Prefab;

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
        Debug.Assert(Prefab is not null, "PrefabSelectButton instance was not correctly set up, Prefab field is null");
    }

    public void SelectPrefab()
    {
        Debug.Log($"Prefab {Prefab} was selected {prefabSelector}");
        prefabSelector.TriggerSelectPrefab(Prefab);
    }
}

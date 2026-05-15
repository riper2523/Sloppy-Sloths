#nullable enable
using UnityEngine;
using Assets.Prefabs.MapBuilder.MapBuilderManager;
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Buttons;

using Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector;
using System;
// namespace Assets.Prefabs.MapBuilder.MapBuilderManager.States
// {
[RequireComponent(typeof(INodeManager))]
public class BuilderModeState : MonoBehaviour, IMapBuilderManagerState
{
    [SerializeField]
    private GameObject? UpperLeftPannelButtonList;

    private INodeManager? NodeManager;

    [SerializeField]
    private PrefabSelector? prefabSelector;

    [SerializeField]
    private RotationManager? _RotationManager;

    [SerializeField]
    private ScalingManager? _ScalingManager;

    [SerializeField]
    private GameObject? _ActivePrefab;

    private enum ModeState
    {
        PLACING_PREFABS,
        MANIPULATING_POLYGONS
    }
    // I choose this path since state pattern would be overkill there
    private ModeState CurrentModeState = ModeState.MANIPULATING_POLYGONS;

    private GameObject? ActivePrefab
    {
        get { return _ActivePrefab; }

        set
        {
            _ActivePrefab = value;
            CurrentModeState = _ActivePrefab is not null ? ModeState.PLACING_PREFABS : ModeState.MANIPULATING_POLYGONS;
        }
    }

    void Awake()
    {
        NodeManager = GetComponentInParent<INodeManager>();
        Debug.Assert(UpperLeftPannelButtonList is not null);
        Debug.Assert(prefabSelector is not null);

        NodeManager.SelectedContainerChanged += newState =>
        {
            UpperLeftPannelButtonList!.SetActive(newState is not null);
            ActivePrefab = null;
        };

        _RotationManager!.ProvidedEvent += rotation =>
        {
            NodeManager.ApplyTransformation(rotation);
        };

        _ScalingManager!.ProvidedEvent += scaling =>
        {
            NodeManager.ApplyTransformation(scaling);
        };
    }

    private void ChoosePrefab(GameObject prefab)
    {
        ActivePrefab = prefab;
        NodeManager!.ResetActivityState();
    }

    public void OnActivateState()
    {
        prefabSelector!.PrefabSelected += ChoosePrefab;
    }

    public void OnDeactivateState()
    {
        prefabSelector!.PrefabSelected -= ChoosePrefab;
    }

    public void EscapeWasClicked()
    {
        ActivePrefab = null;
        if (CurrentModeState == ModeState.MANIPULATING_POLYGONS)
        {
            NodeManager!.ResetActivityState();
        }
    }

    public void VoidWasClicked(Vector3 where)
    {
        if (CurrentModeState == ModeState.PLACING_PREFABS)
        {
            NodeManager!.AddNodeContainerAtPos(ActivePrefab!, where);
        }
        else
        {
            NodeManager!.HandleVoidWasClicked(where);
        }
    }

    public StateID StateType { get => StateID.BUILDER_MODE; }
}
// }

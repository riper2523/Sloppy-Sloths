#nullable enable
using System;
using UnityEngine;
using Assets.Prefabs.MapBuilder.MapBuilderManager;
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Node.SpecialItems;
using Assets.Prefabs.MapBuilder.Buttons;

using Assets.Prefabs.MapBuilder.Popups;

using Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector;
// namespace Assets.Prefabs.MapBuilder.MapBuilderManager.States
// {
// [RequireComponent(typeof(INodeManager))]
public class BuilderModeState : MonoBehaviour, IMapBuilderManagerState
{
    enum Mode
    {
        MODE_INACTIVE,
        PLACING_PREFABS,
        MODIFYING_PREFABS
    }
    // private record State(Mode Mode = Mode.MODIFYING_PREFABS, MapBuilderItemType? CurrentItem = null)
    [Serializable]
    class State : IEquatable<State>
    {
        public Mode Mode { get; }
        public MapBuilderItemType? CurrentItem { get; }

        public bool Equals(State? other)
        {
            if (other is null) return false;
            return Mode == other.Mode && CurrentItem == other.CurrentItem;
        }

        public override bool Equals(object obj) => Equals(obj as State);

        public override int GetHashCode() => HashCode.Combine(Mode, CurrentItem);

        public static bool operator ==(State left, State right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(State left, State right) => !(left == right);

        public override string ToString() => $"State(Mode: {Mode}, CurrentItem: {CurrentItem?.ToString() ?? "null"})";

        public State(Mode Mode = Mode.MODIFYING_PREFABS, MapBuilderItemType? CurrentItem = null)
        {
            this.Mode = Mode;
            this.CurrentItem = CurrentItem;
            Debug.Assert(!(Mode == Mode.PLACING_PREFABS && (CurrentItem is null || CurrentItem == MapBuilderItemType.Unset)),
                    "Cannot enter PLACING_PREFABS mode without a valid selected prefab item type. CurrentItem must be set and cannot be Unset.");
        }
    }

    [SerializeField]
    private MapBuilderItemRegistry? itemRegistry;

    private SpecialItemConfigUI? activeSpecialItemConfigUI;
    private MapBuilderManager? mapBuilderManager;
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
    private GameObject? builderModeMenu;

    [SerializeField]
    private State _currentState = new();

    [SerializeField, Tooltip("Debug view of the current state")]
    private string _debugCurrentState = "";

    private ISpecialItemController? CurrentlySelectedSpecialItem { get; set; }

    State CurrentState
    {
        get => _currentState;

        set
        {
            if (_currentState == value) return;

            var oldState = _currentState;
            Debug.Log($"Changing state to {value}, oldState: {oldState}");
            _currentState = value;
            _debugCurrentState = _currentState.ToString();

            // Update the UI panel for the NEW state FIRST
            // This prevents the SelectedContainerChanged event from recursively altering the state
            if (_currentState.CurrentItem is not null && _currentState.Mode == Mode.MODIFYING_PREFABS)
            {
                var mapping = itemRegistry!.GetMapping(_currentState.CurrentItem.Value);
                ActiveConfigPanel = mapping.ConfigPanel;
            }
            else
            {
                ActiveConfigPanel = null;
            }

            // Deselect polygon if we are switching TO placing prefabs, 
            // OR if we were modifying a polygon and are now switching to something else.
            if (_currentState.Mode == Mode.PLACING_PREFABS ||
               (oldState.Mode == Mode.MODIFYING_PREFABS && oldState.CurrentItem == MapBuilderItemType.Polygon))
            {
                NodeManager!.ResetActivityState();
            }
        }
    }

    void Awake()
    {
        NodeManager = GetComponentInChildren<INodeManager>();
        mapBuilderManager = GetComponent<MapBuilderManager>();

        Debug.Assert(mapBuilderManager is not null);
        Debug.Assert(UpperLeftPannelButtonList is not null);
        Debug.Assert(prefabSelector is not null);
        Debug.Assert(itemRegistry is not null);

        NodeManager.SelectedContainerChanged += newState =>
        {
            if (newState is not null)
            {
                CurrentlySelectedSpecialItem = PolygonSpecialItemController.instance;
                CurrentState = new(Mode.MODIFYING_PREFABS, MapBuilderItemType.Polygon);
            }
            // null safe comparision order
            else if (CurrentState.Mode == Mode.MODIFYING_PREFABS && MapBuilderItemType.Polygon == CurrentState.CurrentItem)
            {
                CurrentState = new(Mode.MODIFYING_PREFABS, null);
            }
        };


        SpecialItemController.SpecialItemSelected += OnSpecialItemSelected;
        SpecialItemController.SpecialItemDeleted += OnSpecialItemDeleted;

        Debug.Assert(_RotationManager is not null);
        Debug.Assert(_ScalingManager is not null);

        _RotationManager!.ProvidedEvent += rotation =>
        {
            NodeManager.ApplyTransformation(rotation);
        };

        _ScalingManager!.ProvidedEvent += scaling =>
        {
            NodeManager.ApplyTransformation(scaling);
        };
    }

    private void OnDestroy()
    {
        SpecialItemController.SpecialItemSelected -= OnSpecialItemSelected;
        SpecialItemController.SpecialItemDeleted -= OnSpecialItemDeleted;
    }

    private void OnSpecialItemDeleted(SpecialItemController item)
    {
        if (object.ReferenceEquals(CurrentlySelectedSpecialItem, item))
        {
            CurrentState = new(Mode.MODIFYING_PREFABS, null);
        }
    }

    private SpecialItemConfigUI? ActiveConfigPanel
    {
        get { return activeSpecialItemConfigUI; }
        set
        {
            if (activeSpecialItemConfigUI == value)
            {
                if (activeSpecialItemConfigUI != null)
                {
                    activeSpecialItemConfigUI.Trigger(CurrentlySelectedSpecialItem);
                }
                return;
            }
            //This can't be simplified!! Look into Unity Fake null problem
            if (activeSpecialItemConfigUI != null)
            {
                activeSpecialItemConfigUI.Trigger(null);
            }
            activeSpecialItemConfigUI = value;
            //This can't be simplified!! look into Unity Fake null problem
            if (activeSpecialItemConfigUI != null)
            {
                Debug.Assert(CurrentlySelectedSpecialItem is not null);
                activeSpecialItemConfigUI.Trigger(CurrentlySelectedSpecialItem);
            }
        }
    }

    // Happens after the item is clicked
    public void OnSpecialItemSelected(SpecialItemController item)
    {
        CurrentlySelectedSpecialItem = item;
        Debug.Log($"Special item selected {item}");
        CurrentState = new(Mode.MODIFYING_PREFABS, item.ItemType);
    }

    private void ChoosePrefab(MapBuilderItemType type)
    {
        Debug.Log($"Prefab chosen {type}");
        CurrentState = new(Mode.PLACING_PREFABS, type);
    }


    public void OnActivateState()
    {
        prefabSelector!.PrefabSelected += ChoosePrefab;
        UpperLeftPannelButtonList?.SetActive(true);
        builderModeMenu?.SetActive(true);
    }

    public void OnDeactivateState()
    {
        prefabSelector!.PrefabSelected -= ChoosePrefab;
        UpperLeftPannelButtonList?.SetActive(false);
        builderModeMenu?.SetActive(false);
        CurrentlySelectedSpecialItem = null;
        CurrentState = new(Mode.MODE_INACTIVE, null);
        NodeManager?.ResetActivityState();
    }

    public void EscapeWasClicked()
    {
        CurrentState = new(Mode.MODIFYING_PREFABS, null);
    }

    private IInputInformation? inputInformation;

    private void Update()
    {
        if (inputInformation == null)
        {
            inputInformation = mapBuilderManager?.GetComponentInChildren<IInputInformation>(true);
        }

        if (inputInformation != null && inputInformation.DelKeyWasClicked())
        {
            if (CurrentlySelectedSpecialItem != null && CurrentlySelectedSpecialItem != PolygonSpecialItemController.instance)
            {
                CurrentlySelectedSpecialItem.Delete();
                CurrentState = new(Mode.MODIFYING_PREFABS, null);
            }
        }
    }

    public void VoidWasClicked(Vector3 where)
    {
        if (CurrentState.Mode == Mode.PLACING_PREFABS)
        {
            if (CurrentState.CurrentItem != null && itemRegistry != null)
            {
                var mapping = itemRegistry.GetMapping(CurrentState.CurrentItem.Value);
                var specialItem = mapping.Prefab.GetComponent<SpecialItemController>();
                if (specialItem != null)
                {
                    var SpecialItemController = mapBuilderManager!.TryAddSpecialItem(specialItem, where);
                    if (SpecialItemController is not null)
                    {
                        CurrentlySelectedSpecialItem = SpecialItemController;
                        CurrentState = new(Mode.MODIFYING_PREFABS, specialItem.ItemType);
                    }
                }
                else
                {
                    NodeManager!.AddNodeContainerAtPos(mapping.Prefab, where);
                }
            }
        }
        else
        {
            NodeManager!.HandleVoidWasClicked(where);
            // CurrentState = new(Mode.MODIFYING_PREFABS, null);
        }
    }

    public StateID StateType { get => StateID.BUILDER_MODE; }
}

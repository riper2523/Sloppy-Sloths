using UnityEngine;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

using Assets.Prefabs.MapBuilder.MapBuilderManager.States;
using Assets.Prefabs.MapBuilder;

using Assets.Prefabs.MapBuilder.Utils;

using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(IInputInformation))]
[RequireComponent(typeof(INodeManager))]
[RequireComponent(typeof(IStateProvider))]
[RequireComponent(typeof(MapMover))]
[RequireComponent(typeof(MapScaler))]
public class MapBuilderManager : MonoBehaviour
{
    private IStateProvider stateProvider;

    [SerializeField]
    private IMapBuilderManagerState _State;

    private MapMover mapMover;

    private MapScaler mapScaler;

    [SerializeField]
    private bool _movingTheMapRightNow;
    private bool MovingTheMapRightNow
    {
        get => _movingTheMapRightNow;

        set
        {
            _movingTheMapRightNow = value;
            enableMovementButton.CanBeMoved = value;

            if (value)
            {
                State.EscapeWasClicked();
            }
        }
    }

    [SerializeField]
    private EnableMovementButtonBase enableMovementButton;

    private IMapBuilderManagerState State
    {
        get => _State;

        set
        {
            _State?.OnDeactivateState();

            _State = value;
            Debug.Assert(value is not null);

            _State.OnActivateState();
        }
    }

    [SerializeField]
    private INodeManager nodeManager;

    [SerializeField]
    private IInputInformation inputInformation;

    void Awake()
    {
        stateProvider = GetComponent<IStateProvider>();
        inputInformation = GetComponent<IInputInformation>();
        mapMover = GetComponent<MapMover>();
        mapScaler = GetComponent<MapScaler>();
    }

    void Start()
    {
        State = stateProvider.GetBuilderModeState();

        enableMovementButton.ProvidedEvent += _ =>
        {
            MovingTheMapRightNow = !MovingTheMapRightNow;
        };

        Debug.Assert(State is not null);
    }

    void Update()
    {
        if (inputInformation.EscapeKeyWasClicked())
        {
            MovingTheMapRightNow = false;
            State.EscapeWasClicked();
        }

        if (inputInformation.IsCtrlPressed())
        {
            mapScaler.ProcessScaling(Camera.main, inputInformation.ScrollValue());
        }

        var areWeOverAGameObject = inputInformation.AreWeOverAGameObject();

        if (MovingTheMapRightNow && !areWeOverAGameObject)
        {
            mapMover.ProcessPanning(Camera.main,
                    inputInformation.IsPressed(),
                    inputInformation.WeClickedThisFrame(),
                    inputInformation.WeReleasedThisFrame(),
                    Mouse.current.position.ReadValue());
            return;
        }

        if (inputInformation.WeClickedThisFrame())
        {
            if (!areWeOverAGameObject)
            {
                State.VoidWasClicked(inputInformation.GetMouseWorldPos());

                Debug.Log($"Void was clicked at {inputInformation.GetMouseWorldPos()}");
            }
            else
            {
                MovingTheMapRightNow = false;

                PointerEventData pointerData = new(EventSystem.current)
                {
                    position = Mouse.current.position.ReadValue()
                };

                List<RaycastResult> results = new();
                EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    Debug.Log($"{results[0].gameObject} was hit");
                }
            }
        }
    }

    public void MoveToBuilderMode()
    {
        if (State.StateType != StateID.BUILDER_MODE)
        {
            State = stateProvider.GetBuilderModeState();
        }
    }

    public void MoveToGearSelectionMode()
    {
        if (State.StateType != StateID.GEAR_SELECT_MODE)
        {
            State = stateProvider.GetGearSelectModeState();
        }
    }

    public void MoveToTestingMode()
    {
        if (State.StateType != StateID.TESTING_MODE)
        {
            State = stateProvider.GetTestingModeState();
        }
    }
}

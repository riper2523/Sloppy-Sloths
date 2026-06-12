using UnityEngine;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

public class TestingModeState : MonoBehaviour, IMapBuilderManagerState
{
    public StateID StateType => StateID.TESTING_MODE;
    public void OnActivateState() { }
    public void OnDeactivateState() { }
}

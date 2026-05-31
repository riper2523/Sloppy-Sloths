using UnityEngine;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

// namespace ajs
// {
public class GearSelectionState : MonoBehaviour, IMapBuilderManagerState
{
    [SerializeField] private GameObject gearSelectionMenu;

    public StateID StateType => StateID.GEAR_SELECT_MODE;
    public void OnActivateState() 
    { 
        if (gearSelectionMenu != null) gearSelectionMenu.SetActive(true);
    }
    public void OnDeactivateState() 
    { 
        if (gearSelectionMenu != null) gearSelectionMenu.SetActive(false);
    }
}
// }

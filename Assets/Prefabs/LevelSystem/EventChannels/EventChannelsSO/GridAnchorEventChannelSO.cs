using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Grid Anchor Event Channel")]
public class GridAnchorEventChannelSO : ScriptableObject
{
    public UnityAction<GridAnchor> OnEventRaised;
    public void RaiseEvent(GridAnchor anchor) => OnEventRaised?.Invoke(anchor);
}

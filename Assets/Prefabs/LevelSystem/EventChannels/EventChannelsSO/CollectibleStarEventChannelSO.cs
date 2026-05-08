using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Collectible Star Event Channel")]
public class CollectibleStarEventChannelSO : ScriptableObject
{
    public UnityAction<CollectibleStar> OnEventRaised;
    public void RaiseEvent(CollectibleStar star) => OnEventRaised?.Invoke(star);
}

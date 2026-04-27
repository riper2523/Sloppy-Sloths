using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Level Data Event Channel")]
public class LevelDataEventChannelSO : ScriptableObject
{
    public UnityAction<LevelData> OnEventRaised;
    public void RaiseEvent(LevelData data) => OnEventRaised?.Invoke(data);
}

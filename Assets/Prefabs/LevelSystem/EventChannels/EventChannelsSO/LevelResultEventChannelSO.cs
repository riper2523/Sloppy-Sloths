using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Level Result Event Channel")]
public class LevelResultEventChannelSO : ScriptableObject
{
    public UnityAction<LevelResult> OnEventRaised;
    public void RaiseEvent(LevelResult result) => OnEventRaised?.Invoke(result);
}

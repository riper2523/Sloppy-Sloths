using UnityEngine;

public class LevelStarter : MonoBehaviour
{
    [Header("Shared Memory")]
    [SerializeField] private CurrentSessionSO currentSession;

    [Header("Broadcasting On")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;

    [Header("Listening To")]
    [SerializeField] private VoidEventChannelSO restartLevelEvent;

    private void Start()
    {
        if (currentSession != null && currentSession.activeLevel != null)
        {
            loadLevelEvent.RaiseEvent(currentSession.activeLevel);
        }
        else
        {
            Debug.LogWarning("No active level found in CurrentSessionSO! Map will not load automatically.");
        }
    }

    private void OnEnable() => restartLevelEvent.OnEventRaised += HandleRestart;
    private void OnDisable() => restartLevelEvent.OnEventRaised -= HandleRestart;

    private void HandleRestart()
    {
        if (currentSession != null && currentSession.activeLevel != null)
        {
            loadLevelEvent.RaiseEvent(currentSession.activeLevel);
        }
    }
}

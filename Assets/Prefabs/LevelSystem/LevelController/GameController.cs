using UnityEngine;

public class LevelStarter : MonoBehaviour
{
    [Header("Shared Memory")]
    [SerializeField] private CurrentSessionSO currentSession;

    [Header("Broadcasting On")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;

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
}

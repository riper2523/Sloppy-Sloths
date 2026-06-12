using UnityEngine;

public class LevelStarter : MonoBehaviour
{
    [Header("Shared Memory")]
    [SerializeField] private CurrentSessionSO currentSession;

    [Header("Broadcasting On")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;

    private void Start()
    {
        if (MapLoader.PendingLevel != null)
        {
            if (currentSession != null)
            {
                currentSession.activeLevel = MapLoader.PendingLevel;
            }
            loadLevelEvent.RaiseEvent(MapLoader.PendingLevel);
            MapLoader.PendingLevel = null;
        }
        else if (currentSession != null && currentSession.activeLevel != null)
        {
            loadLevelEvent.RaiseEvent(currentSession.activeLevel);
        }
        else
        {
            Debug.LogWarning("No active level found in MapLoader.PendingLevel or CurrentSessionSO! Map will not load automatically.");
        }
    }
}

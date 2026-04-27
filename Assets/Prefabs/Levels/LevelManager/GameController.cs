using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private LevelData currentLevelData;

    [Header("Broadcasting On")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;

    [Header("Listening To")]
    [SerializeField] private VoidEventChannelSO restartLevelEvent;

    private void Start()
    {
        if (currentLevelData != null)
        {
            loadLevelEvent.RaiseEvent(currentLevelData);
        }
    }
}

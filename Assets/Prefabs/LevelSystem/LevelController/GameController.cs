using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private LevelData currentLevelData;

    [Header("Broadcasting On")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;

    private void Start()
    {
        if (currentLevelData != null)
        {
            loadLevelEvent.RaiseEvent(currentLevelData);
        }
    }
}

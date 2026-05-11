using UnityEngine;

public class DeathZoneHandler : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO restartLevelEvent;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth"))
        {
            restartLevelEvent.RaiseEvent();
        }
    }
}

using UnityEngine;

public class DeathZoneHandler : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO restartLevelEvent;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth") || (other.transform.parent != null && other.transform.parent.CompareTag("Sloth")))
        {
            restartLevelEvent.RaiseEvent();
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}

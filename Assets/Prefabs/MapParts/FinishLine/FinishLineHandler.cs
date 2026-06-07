using UnityEngine;

public class FinishLineHandler : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO finishLineCrossedEvent;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth") || (other.transform.parent != null && other.transform.parent.CompareTag("Sloth")))
        {
            finishLineCrossedEvent.RaiseEvent();
        }
    }
}

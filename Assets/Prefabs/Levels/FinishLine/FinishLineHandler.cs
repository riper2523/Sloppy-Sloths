using UnityEngine;

public class FinishLineHandler : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth"))
        {
            GameEvents.OnFinishLineCrossed?.Invoke();
        }
    }
}

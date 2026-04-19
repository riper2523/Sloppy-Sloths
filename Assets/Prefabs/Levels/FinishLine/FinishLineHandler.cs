using UnityEngine;
using UnityEngine.Events;

public class FinishLineHandler : MonoBehaviour
{
    public UnityEvent finishLineCrossed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth"))
        {
            finishLineCrossed.Invoke();
        }
    }
}

using UnityEngine;
using UnityEngine.Events;

public class FinishLineHandler : MonoBehaviour
{
    public UnityEvent finishLineCrossed;
    private int crossedCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth") && crossedCount == 0)
        {
            crossedCount++;
            finishLineCrossed.Invoke();
        }
    }
}

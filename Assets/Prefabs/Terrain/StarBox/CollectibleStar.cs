using UnityEngine;

public class CollectibleStar : MonoBehaviour
{
    [SerializeField] private int starID; 
    [SerializeField] private IntEventChannelSO starCollectedEvent;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth") || other.CompareTag("Vehicle"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        starCollectedEvent.RaiseEvent(starID);
        Destroy(gameObject);
    }
}

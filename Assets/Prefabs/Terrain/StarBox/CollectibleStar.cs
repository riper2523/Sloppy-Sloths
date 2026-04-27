using UnityEngine;

public class CollectibleStar : MonoBehaviour
{
    [SerializeField] private int starID; 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth") || other.CompareTag("Vehicle"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        Debug.Log($"Star {starID} collected!");
        GameEvents.OnStarCollected?.Invoke(starID);
        Destroy(gameObject);
    }
}

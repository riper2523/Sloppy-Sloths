using System.Xml.Serialization;
using UnityEngine;

public class CollectibleStar : MonoBehaviour
{
    [Header("Broadcasting On")]
    [SerializeField] private CollectibleStarEventChannelSO starSpawnedEvent;
    [SerializeField] private CollectibleStarEventChannelSO starCollectedEvent;
    public int starID;
    private bool isCollected = false;

    private void Start()
    {
        starSpawnedEvent.RaiseEvent(this);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth") || other.CompareTag("Vehicle"))
        {
            Collect();
        }
    }

    public void SetCollected()
    {
        isCollected = true;
        gameObject.SetActive(false);
    }

    public void Collect()
    {
        if (isCollected) return;
        starCollectedEvent.RaiseEvent(this);
    }
}

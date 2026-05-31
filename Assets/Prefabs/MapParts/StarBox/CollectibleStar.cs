using System.Xml.Serialization;
using UnityEngine;

public class CollectibleStar : MonoBehaviour
{
    [Header("Broadcasting On")]
    [SerializeField] private CollectibleStarEventChannelSO starSpawnedEvent;
    [SerializeField] private CollectibleStarEventChannelSO starCollectedEvent;
    [SerializeField] private AudioClip collectSound;
    public int starID;
    private bool isCollected = false;

    private void Start()
    {
        starSpawnedEvent.RaiseEvent(this);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sloth") || (other.transform.parent != null && other.transform.parent.CompareTag("Sloth")))
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
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }
}

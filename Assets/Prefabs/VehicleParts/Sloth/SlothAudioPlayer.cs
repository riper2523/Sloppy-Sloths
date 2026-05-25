using UnityEngine;

public class SlothAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude > 3f)
        {
            audioSource.Play();
        }
    }
}

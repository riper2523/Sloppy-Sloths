using UnityEngine;

public class WheelSound : MonoBehaviour
{
    [Header("Komponenty")]
    public AudioSource audioSource;
    public Rigidbody2D rb;

    [Header("Prędkość koła")]
    public float minSpeed = 10f; // Prędkość, od której w ogóle zaczynamy grać
    public float maxSpeed = 1000f; // Prędkość maksymalna (przy której dźwięk osiąga maksimum)

    [Header("Prędkość dźwięku (Pitch)")]
    public float minPitch = 0.8f; // Prędkość audio przy powolnym toczeniu (lekko zwolnione)
    public float maxPitch = 2.5f; // Prędkość audio przy maksymalnych obrotach (bardzo szybkie)

    [Header("Głośność")]
    public float minVolume = 0.05f;
    public float maxVolume = 0.3f;

    private void Update()
    {
        float currentSpeed = Mathf.Abs(rb.angularVelocity);

        if (currentSpeed > minSpeed)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // 1. Obliczamy "procent" prędkości koła (wynik zawsze między 0.0 a 1.0)
            // Używamy InverseLerp, żeby sprawdzić w którym miejscu między minSpeed a maxSpeed jesteśmy.
            float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);

            // 2. Tłumaczymy ten procent na prędkość odtwarzania dźwięku (Pitch)
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedPercent);

            // 3. Tłumaczymy ten procent na głośność (Volume) - szybsze koło = głośniejsze koło
            audioSource.volume = Mathf.Lerp(minVolume, maxVolume, speedPercent);
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }
}
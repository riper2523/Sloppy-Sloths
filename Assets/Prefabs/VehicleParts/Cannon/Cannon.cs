using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Ustawienia Strzału")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootForce = 20f;

    [Header("Referencje")]
    [Tooltip("Pusty obiekt (Empty) umieszczony na samym końcu lufy")]
    [SerializeField] private Transform firePoint;

    // Opcjonalnie: Efekt wizualny wystrzału
    // [SerializeField] private ParticleSystem muzzleFlash;

    public void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Brakuje prefabu pocisku lub FirePointa w armacie!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation, transform.parent);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddForce(firePoint.right * shootForce, ForceMode2D.Impulse);
        }

        // 4. (Opcjonalnie) Odtwarzamy cząsteczki dymu/ognia
        // if (muzzleFlash != null) muzzleFlash.Play();
    }
}
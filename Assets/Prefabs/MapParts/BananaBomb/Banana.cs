using UnityEngine;

public class Banana : MonoBehaviour
{
    [Header("Ustawienia Głównego Wybuchu")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float power = 50f;
    [SerializeField] private float impactThreshold = 4f;
    [SerializeField] private GameObject explosionEffect;

    [Header("Ustawienia Banana Bomby (Dzieci)")]
    [SerializeField] private GameObject spawnChildren;
    [SerializeField] private int childrenCount = 5;
    [SerializeField] private float scatterForce = 15f;

    [Header("Ustawienia Zapalnika")]
    [Tooltip("Ile sekund po zespawnowaniu banan ignoruje uderzenia i tylko się odbija?")]
    [SerializeField] private float armingDelay = 0.5f;

    private bool exploded = false;
    private float spawnTime; // Zmienna zapamiętująca, kiedy obiekt powstał

    private void Start()
    {
        // Zapisujemy dokładny czas, w którym banan pojawił się na planszy
        spawnTime = Time.time;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // 1. KRYTYCZNA ZMIANA: Jeśli od pojawienia się banana nie minął czas uzbrojenia,
        // natychmiast przerywamy funkcję. Banan po prostu się odbije!
        if (Time.time < spawnTime + armingDelay)
        {
            return;
        }

        // 2. Kiedy banan jest już uzbrojony, sprawdzamy siłę uderzenia
        if (!exploded && col.relativeVelocity.magnitude > impactThreshold)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity, transform.parent);
        }

        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D obj in objects)
        {
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = obj.transform.position - transform.position;
                float distance = dir.magnitude;

                if (distance > 0)
                {
                    float wearoff = 1 - (distance / radius);
                    rb.AddForce(dir.normalized * (power * wearoff), ForceMode2D.Impulse);
                }
            }

            if (obj.TryGetComponent(out TNTBlock otherTNT))
            {
                otherTNT.Explode();
            }
        }

        if (spawnChildren != null)
        {
            for (int i = 0; i < childrenCount; i++)
            {
                GameObject miniBanana = Instantiate(spawnChildren, transform.position, Quaternion.identity, transform.parent);
                Rigidbody2D rb = miniBanana.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    Vector2 randomDirection = Random.insideUnitCircle;
                    randomDirection.y = Mathf.Abs(randomDirection.y) + 0.5f;

                    rb.AddForce(randomDirection.normalized * scatterForce, ForceMode2D.Impulse);
                    rb.AddTorque(Random.Range(-20f, 20f), ForceMode2D.Impulse);
                }
            }
        }

        Destroy(gameObject);
    }
}
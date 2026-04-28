using UnityEngine;

public class TNTBlock : MonoBehaviour
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private float power = 50f; 
    [SerializeField] private float impactThreshold = 4f;
    [SerializeField] private GameObject explosionEffect;

    private bool exploded = false;

    private void OnCollisionEnter2D(Collision2D col)
    {
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
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
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

        Destroy(gameObject);
    }
}

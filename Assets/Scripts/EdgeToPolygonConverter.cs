using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
public class EdgeToPolygonConverter : MonoBehaviour
{
    public float thickness = 1f;

    void Start()
    {
        Convert();
    }

#if UNITY_EDITOR
    void Update()
    {
        // optional live update while editing
        Convert();
    }
#endif

    void Convert()
    {
        EdgeCollider2D edge = GetComponent<EdgeCollider2D>();
        PolygonCollider2D poly = GetComponent<PolygonCollider2D>();

        Vector2[] top = edge.points;
        if (top.Length < 2) return;

        Vector2[] shape = new Vector2[top.Length * 2];

        // top side (forward)
        for (int i = 0; i < top.Length; i++)
        {
            shape[i] = top[i];
        }

        // bottom side (reverse + offset down)
        for (int i = 0; i < top.Length; i++)
        {
            int j = top.Length - 1 - i;
            Vector2 p = top[j];

            shape[top.Length + i] = p + Vector2.down * thickness;
        }

        poly.SetPath(0, shape);
    }
}
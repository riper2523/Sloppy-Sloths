using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class EdgeToLineRenderer : MonoBehaviour
{
    void Update()
    {
        var edge = GetComponent<EdgeCollider2D>();
        var line = GetComponent<LineRenderer>();

        Vector2[] points = edge.points;

        line.positionCount = points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            line.SetPosition(i, transform.TransformPoint(points[i]));
        }
    }
}

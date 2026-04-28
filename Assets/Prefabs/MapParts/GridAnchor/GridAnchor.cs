using UnityEngine;

public class GridAnchor : MonoBehaviour
{
    [Header("Grid Dimensions")]
    public int gridSizeX = 5;
    public int gridSizeY = 5;

    private void OnDrawGizmos()
    {
        Vector3 center = transform.position + new Vector3(gridSizeX / 2f, gridSizeY / 2f, 0);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(gridSizeX, gridSizeY, 0));
        
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}

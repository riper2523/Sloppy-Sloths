using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "PartData", menuName = "Scriptable Objects/PartData")]
public class PartData : ScriptableObject
{
    [System.Serializable]
    public struct AimState
    {
        public int angle;
        public TileBase visualTile;
    }
    public GameObject partPrefab;
    public Sprite partSpriteUI;
    public string partName;
    public Tile partTile;
    [Header("Ustawienia celowania")]
    public bool isAimable = false;
    public AimState[] aimStates;
    public int layer = 0;
    [Tooltip("List of layers that can be with this part on same square")]
    public List<int> acceptedLayers = new List<int>();
    [Tooltip("0: Up, 1: Right, 2: Down, 3: Left")]
    public bool[] attachable = new bool[4];
    private void OnValidate()
    {
        if (attachable.Length != 4)
        {
            Debug.LogWarning("The 'attachable' array must have exactly 4 elements.");
            System.Array.Resize(ref attachable, 4);
        }
    }
    public bool HasAttachment(int worldDirection, int rotation)
    {
        rotation = rotation % 4;
        int originalDirection = (worldDirection - rotation + 4) % 4;
        return attachable[originalDirection];
    }
}

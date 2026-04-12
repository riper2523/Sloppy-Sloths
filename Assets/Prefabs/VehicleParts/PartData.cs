using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "PartData", menuName = "Scriptable Objects/PartData")]
public class PartData : ScriptableObject
{
    public GameObject partPrefab;
    public Sprite partSpriteUI;
    public string partName;
    public Tile partTile;
}

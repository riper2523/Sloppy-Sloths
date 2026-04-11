using UnityEngine;
using Assets.Scripts.MapBuilder;
using Grid = Assets.Scripts.MapBuilder.Grid;

using ObjType = UnityEngine.GameObject;
// T stands for object type
[System.Serializable]
public class MapState
{
    [SerializeField]
    private Grid buildingGrid = new();

    [SerializeField]
    private Grid finishLine = new();

    [SerializeField]
    private SerializableDictionary<Point2D, ObjType> mapData = new();

    public Grid BuildingGrid => buildingGrid;
    public Grid FinishLine => finishLine;
    public SerializableDictionary<Point2D, ObjType> MapData => mapData;

    public bool AddObject(ObjType obj, Point2D where)
    {
        return mapData.TryAdd(where, obj);
    }

    public bool RemoveObject(Point2D fromWhere)
    {
        return mapData.Remove(fromWhere);
    }
}

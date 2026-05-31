using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Node.SpecialItems
{
    public class PolygonSpecialItemController : ISpecialItemController
    {
        public void Select()
        {

        }

        public static event System.Action PolygonDeleted;

        public void Delete()
        {
            PolygonDeleted?.Invoke();
        }

        public static readonly PolygonSpecialItemController instance = new();
    }
}

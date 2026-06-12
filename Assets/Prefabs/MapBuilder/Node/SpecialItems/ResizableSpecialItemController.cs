using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Node.SpecialItems
{
    public interface IResizableSpecialItemController : ISpecialItemController
    {
        (uint GridWidth, uint GridHeight) Dimensions { get; set; }
        GameObject gameObject { get; }
    }
}

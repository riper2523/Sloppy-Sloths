using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Node.SpecialItems
{
    public class FinishLineSpecialItemController : SpecialItemController, IResizableSpecialItemController
    {
        public uint GridWidth = 2;
        public uint GridHeight = 4;

        public (uint GridWidth, uint GridHeight) Dimensions
        {
            get => (GridWidth, GridHeight);
            set => SetDimensions(value.GridWidth, value.GridHeight);
        }

        private SpriteRenderer sr;
        private BoxCollider2D bc;

        protected override void Awake()
        {
            base.Awake();

            sr = GetComponentInChildren<SpriteRenderer>();
            bc = GetComponentInChildren<BoxCollider2D>();

            Debug.Assert(sr is not null);
            Debug.Assert(bc is not null);

            sr.size = new Vector2(GridWidth, GridHeight);
            bc.size = new Vector2(GridWidth, GridHeight);
        }

        private void SetDimensions(uint width, uint height)
        {
            GridWidth = width;
            GridHeight = height;

            sr.size = new Vector2(GridWidth, GridHeight);
            bc.size = new Vector2(GridWidth, GridHeight);
        }
    }
}

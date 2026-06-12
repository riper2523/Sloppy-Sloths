using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Prefabs.MapBuilder.MapBuilderManager.PrefabSelector;

namespace Assets.Prefabs.MapBuilder.Node.SpecialItems
{
    public interface ISpecialItemController
    {
        public void Select();
        public void Delete();
    }

    [RequireComponent(typeof(Draggable))]
    [RequireComponent(typeof(Collider2D))]
    public class SpecialItemController : MonoBehaviour, ISpecialItemController, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        public MapBuilderItemType ItemType;

        public static event Action<SpecialItemController> SpecialItemSelected;
        public static event Action<SpecialItemController> SpecialItemDeleted;

        private Draggable draggable;

        protected virtual void Awake()
        {
            draggable = GetComponent<Draggable>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Needed to capture pointer events
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!eventData.dragging)
            {
                Select();
            }
        }

        public void Select()
        {
            SpecialItemSelected?.Invoke(this);
        }

        public void Delete()
        {
            SpecialItemDeleted?.Invoke(this);
            Destroy(gameObject);
        }
    }
}

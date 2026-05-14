using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Prefabs.MapBuilder;
using System;

namespace Assets.Prefabs.MapBuilder.Node
{
    [RequireComponent(typeof(Draggable))]
    class NodeController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, INodeHandle
    {
        private static readonly string OutlineCircleObjectName = "OutlineCircle";

        private GameObject selectedOutline;

        // This can't be null
#nullable enable
        // Null object pattern
        private IDraggable? draggable;
        public event Action? NodeTriggered;
        public event NodeDraggedHandler? NodeDragged;
        public event Action? NodeChangedSelectionState;
        public event Action? NodeDragEnded;

        void Awake()
        {
            draggable = GetComponent<IDraggable>();

            selectedOutline = transform.Find(OutlineCircleObjectName).gameObject;
            Debug.Assert(selectedOutline is not null, $"There is no object named {OutlineCircleObjectName} in the the transform");
        }

        void Start()
        {
            Active = false;
            draggable!.nodeDragged += (oldPos, offset) =>
            {
                NodeDragged?.Invoke(oldPos, offset);
            };

            draggable!.NodeDragEnded += () => NodeDragEnded?.Invoke();
        }

        private bool active;

        public bool Active
        {
            get => active;

            set
            {
                if (value == active)
                {
                    return;
                }

                NodeChangedSelectionState?.Invoke();
                active = value;
                selectedOutline.SetActive(value);
                Debug.Log($"Node set to {value}");
            }
        }

        // OnPointerDown is necessary for OnPointerUp to function
        public void OnPointerDown(PointerEventData eventData)
        {
            NodeTriggered?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public Vector3 GetCoordinates()
        {
            return transform.position;
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        public void MoveByOffset(Vector3 offset)
        {
            var actPos = GetCoordinates();
            Debug.Log($"Act pos: {actPos}, offset: {offset}");
            transform.position = actPos + offset;
        }
    }
}

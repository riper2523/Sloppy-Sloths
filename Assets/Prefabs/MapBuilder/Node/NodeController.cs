using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using System;
using Assets.Prefabs.MapBuilder.Serialization;

namespace Assets.Prefabs.MapBuilder.Node
{
    [System.Serializable]
    public struct NodeControllerDTO : INodeHandleDTO
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public readonly Vector3 AsVector3()
        {
            return new(x, y, z);
        }
        public NodeControllerDTO(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public readonly NodeHandleType Type => NodeHandleType.CIRCULAR;
    }


    [RequireComponent(typeof(Draggable))]
    class NodeController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, INodeHandle
    {
        private static readonly string OutlineCircleObjectName = "OutlineCircle";

        private GameObject selectedOutline;

        // This can't be null
#nullable enable
        private IDraggable? draggable;
        public event Action? NodeTriggered;
        public event NodeDraggedHandler? NodeDragged;
        public event Action? NodeChangedSelectionState;
        public event Action? NodeDragEnded;

        public Vector3 Coordinates { get => transform.position; set => transform.position = value; }

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

        public void Delete()
        {
            Destroy(gameObject);
        }

        public void MoveByOffset(Vector3 offset)
        {
            var actPos = Coordinates;
            Debug.Log($"Act pos: {actPos}, offset: {offset}");
            transform.position = actPos + offset;
        }

        public INodeHandleDTO SerializeToDTO()
        {
            return new NodeControllerDTO(Coordinates);
        }

        public void SetUpUsingDTO(INodeHandleDTO nodeHandleDTO)
        {
            if (nodeHandleDTO is NodeControllerDTO nodeControllerDTO)
            {
                Coordinates = nodeControllerDTO.AsVector3();
            }
            else
            {
                Debug.LogError($"Invalid type of DTO which declared {nodeHandleDTO.Type} type");
            }
        }
    }
}

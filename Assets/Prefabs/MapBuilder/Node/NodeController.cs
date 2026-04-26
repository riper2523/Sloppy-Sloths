using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Prefabs.MapBuilder.Node
{
    class NodeController : MonoBehaviour, IPointerClickHandler, INodeHandle
    {
        private static readonly string OutlineCircleObjectName = "OutlineCircle";

        private Vector3 lastPosition;

        private Collider2D nodeCollider;

        private GameObject selectedOutline;

        //TODO: think about making this a serializedField
        private Draggable draggable;


        private bool _isActive;

        public event NodeTriggeredHandler? NodeSelected;
        public event NodeTriggeredHandler? NodeMoved;

        void Awake()
        {
            nodeCollider = GetComponent<Collider2D>();
            draggable = GetComponent<Draggable>();
            selectedOutline = transform.Find(OutlineCircleObjectName).gameObject;
            lastPosition = transform.position;
        }

        void Start()
        {
            NodeSelected?.Invoke(this);
        }

        public bool Active
        {
            get => _isActive;

            set
            {
                if (value == _isActive)
                {
                    return;
                }

                //TODO: think about this
                // draggable.enabled = value;
                _isActive = value;
                Debug.Log("Setting the outline to active");
                selectedOutline.SetActive(value);
                Debug.Log($"Node set to {value}");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            NodeSelected?.Invoke(this);
        }

        [SerializeField] private float deltaMoved = 0.0001f;
        public void Update()
        {
            if ((transform.position -
        lastPosition).sqrMagnitude > deltaMoved *
        deltaMoved)
            {
                lastPosition = transform.position;
                Debug.Log("Node moved");
                NodeMoved?.Invoke(this);
            }
        }

        public Vector3 GetCoordinates()
        {
            return transform.position;
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        // public INodeHandle GetPrimaryActiveNode()
        // {
        //     if (ActiveNodes == null || ActiveNodes.Count == 0)
        //     {
        //         return null;
        //     }
        //     return ActiveNodes[0];
        // }
    }
}

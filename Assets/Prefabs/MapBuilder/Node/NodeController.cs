using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Prefabs.MapBuilder.Node
{
    class NodeController : MonoBehaviour, IPointerClickHandler, INodeHandle
    {
        private static readonly string OutlineCircleObjectName = "OutlineCircle";

        private Vector3 lastPosition;
        [SerializeField] private float epsilon = 0.0001f;

        private Collider2D nodeCollider;

        private GameObject selectedOutline;

        //TODO: think about making this a serializedField
        private IInputInformation inputInformation;

        //TODO: think about making this a serializedField
        private Draggable draggable;

        //TODO: think about making this a serializedField, and removing the method below
        private INodeContainer nodeContainer;

        private bool wasSetUp = false;

        private bool _isActive;

        void Awake()
        {
            nodeCollider = GetComponent<Collider2D>();
            draggable = GetComponent<Draggable>();
            selectedOutline = transform.Find(OutlineCircleObjectName).gameObject;
            lastPosition = transform.position;
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

                if (value && nodeContainer is not null)
                {
                    // If we can't activate the node then we should deactivate it
                    value = nodeContainer.TryActivatingTheNode(this);
                }

                //TODO: think about this
                // draggable.enabled = value;
                _isActive = value;
                selectedOutline.SetActive(value);
                Debug.Log($"Node set to {value}");
            }
        }

        public void SetTheNodeUp(INodeContainer nodeContainer, IInputInformation inputInformation)
        {
            this.nodeContainer = nodeContainer;
            this.inputInformation = inputInformation;
            wasSetUp = true;
        }


        public bool DoesCollide(Vector3 point)
        {
            Collider2D hitCollider = Physics2D.OverlapPoint(point);

            if (hitCollider != null && hitCollider == nodeCollider)
            {
                return true;
            }
            return false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Active = true;
        }

        public void Update()
        {
            if (!wasSetUp)
            {
                return;
            }

            if (Active && inputInformation.DelKeyWasClicked())
            {
                TryDeletingTheNode();
            }

            if ((transform.position -
        lastPosition).sqrMagnitude > epsilon *
        epsilon)
            {
                lastPosition = transform.position;
                Debug.Log("Node moved");
                nodeContainer.NodeMoved(this);
            }
        }

        private void TryDeletingTheNode()
        {
            if (nodeContainer.TryDeletingThis(this))
            {
                Debug.Log("Object deleted");
                Destroy(gameObject);
            }
        }

        public Vector3 GetCoordinates()
        {
            return transform.position;
        }
    }
}


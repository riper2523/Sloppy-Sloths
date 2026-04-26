using System.Collections.Generic;
using UnityEngine;
using Assets.Prefabs.MapBuilder;
using UnityEngine.EventSystems;

class NodeManager : MonoBehaviour, INodeManager
{
    //TODO: Fix tightly coupled code
    private INodeContainer activeNodeContainer;
    private readonly List<INodeHandle> nodes = new();

    [SerializeField] public float additionThreeshold = 0.5f;

    [SerializeField] private GameObject gameContainerPrefab;

    [SerializeField] public float addingPointThreeshold = 1f;

    //TODO: think about making this a [SerializeField]
    private IInputInformation inputInformation;

    void Awake()
    {
        Debug.Assert(gameContainerPrefab is not null);
        inputInformation = GetComponent<IInputInformation>();
    }

    void Start()
    {
        AddNodeContainerAtPos(gameContainerPrefab, new(85, -35));
    }

    void Update()
    {
        if (inputInformation.EscapeKeyWasClicked())
        {
            activeNodeContainer?.ResetActivityState();
            activeNodeContainer = null;
            return;
        }

        if (inputInformation.DelKeyWasClicked() && activeNodeContainer != null)
        {
            //TODO: remove code coupling which causes making this assert
            Debug.Assert(activeNodeContainer.ActivityState.IsContainerActive());
            if (!activeNodeContainer.ActivityState.AreAnyNodesActive())
            {
                activeNodeContainer.Delete();
                // Debug.LogError($"Failed to delete node from the container {activeNodeContainer}");
                return;
            }

            activeNodeContainer.DeletePrimaryNode();
        }

        if (inputInformation.VoidWasClicked())
        {
            HandleVoidWasClicked(inputInformation.GetMouseWorldPos());
        }
    }

    private void HandleVoidWasClicked(Vector2 position)
    {
        if (activeNodeContainer == null)
        {
            return;
        }

        if (activeNodeContainer.ActivityState.IsContainerActive())
        {
            activeNodeContainer.ResetActivityState();
        }

        var mousePos = position;
        if (!activeNodeContainer.ActivityState.AreAnyNodesActive())
        {
            var closestOnCollider = activeNodeContainer.GetClosestPointOnCollider(mousePos);
            Debug.Log($"Closest on collider: {closestOnCollider}");
            if (Vector2.Distance(mousePos, closestOnCollider) > addingPointThreeshold)
            {
                activeNodeContainer?.ResetActivityState();
                activeNodeContainer = null;
                return;
            }

            //TODO: Prevent the symoultaneous addition and selection, prevent addition of more than one node at a time
            var newNodeController = activeNodeContainer.TryAddingNodeAtPoint(closestOnCollider);

            if (newNodeController == null)
            {
                Debug.LogError($"Failed to add point to container: {activeNodeContainer}");
                return;
            }
        }
    }

    public INodeContainer AddNodeContainerAtPos(GameObject nodeContainerPrefab, Vector2 position)
    {
        var container = Instantiate(nodeContainerPrefab, position, Quaternion.identity, transform);
        var containerController = container.GetComponent<INodeContainer>();
        if (containerController is null)
        {
            Debug.LogError($"Prefab '{nodeContainerPrefab.name}' is missing NodeController.", container);
            Destroy(container);
            return null;
        }

        containerController.NodeSelected += (container, node) =>
        {
            if (inputInformation.IsCtrlPressed())
            {
                container.HandleNodeSelection(node);
            }
            else
            {
                Debug.Log("Node selected triggered");
                //TODO: fix this
                container.ResetActivityState();
                container.HandleNodeSelection(node);
            }

            activeNodeContainer = container;
        };

        containerController.NodeAdditionRequested += (container, position) =>
        {
            if (activeNodeContainer != container)
            {
                Debug.LogError("Node addition requested by inactive container");
                return;
            }

            var node = container.TryAddingNodeAtPoint(position);
            if (node == null)
            {
                Debug.LogError($"Failed to add point add position {position}");
            }
        };

        containerController.ContainerSelected += () =>
        {
            activeNodeContainer?.ResetActivityState();
            activeNodeContainer = containerController;
            containerController.HandleContainerSelection();
        };

        activeNodeContainer = containerController;
        Debug.Log("Container is about to get activated");
        container.SetActive(true);

        return containerController;
    }
}

#nullable enable
using UnityEngine;
using Assets.Prefabs.MapBuilder;

[RequireComponent(typeof(IInputInformation))]
class NodeManager : MonoBehaviour, INodeManager
{
    //TODO: Fix tightly coupled code
    private INodeContainer? activeNodeContainer;

    private void SetActiveNodeContainer(INodeContainer? nodeContainer = null)
    {
        Debug.Log($"Selecting container {activeNodeContainer} {nodeContainer}");
        if (activeNodeContainer == nodeContainer)
        {
            return;
        }

        activeNodeContainer?.ResetActivityState();
        activeNodeContainer = nodeContainer;
        activeNodeContainer?.SelectContainer();
    }

    [SerializeField] public float addingPointThreshold = 1f;

    //TODO: think about making this a [SerializeField]
    private IInputInformation? inputInformation;

    void Awake()
    {
        inputInformation = GetComponent<IInputInformation>();
    }

    void Update()
    {
        if (inputInformation!.EscapeKeyWasClicked())
        {
            Debug.Log($"Escape was clicked activeNodeContainer: {activeNodeContainer}");
            SetActiveNodeContainer();
            return;
        }

        if (inputInformation.VoidWasClicked())
        {
            HandleVoidWasClicked(inputInformation.GetMouseWorldPos());
        }
    }

    private void HandleVoidWasClicked(Vector2 position)
    {
        if (activeNodeContainer is null)
        {
            return;
        }

        var mousePos = position;
        if (!activeNodeContainer.ActivityState.AreAnyNodesActive())
        {
            var closestOnCollider = activeNodeContainer.GetClosestPointOnCollider(mousePos);
            Debug.Log($"Closest on collider: {closestOnCollider}");
            if (Vector2.Distance(mousePos, closestOnCollider) > addingPointThreshold)
            {
                SetActiveNodeContainer(null);
                return;
            }

            var newNodeController = activeNodeContainer.TryAddingNodeAtPoint(closestOnCollider);

            if (newNodeController == null)
            {
                Debug.LogError($"Failed to add point to container: {activeNodeContainer}");
                return;
            }
        }
    }

    public INodeContainer? AddNodeContainerAtPos(GameObject nodeContainerPrefab, Vector3 position)
    {
        var container = Instantiate(nodeContainerPrefab, position, Quaternion.identity, transform);
        var containerController = container.GetComponent<INodeContainer>();
        if (containerController is null)
        {
            Debug.LogError($"Prefab '{nodeContainerPrefab.name}' is missing a component implementing INodeContainer (for example, PolygonBuilder).", container);
            Destroy(container);
            return null;
        }

        containerController.NodeChangedState += (node) =>
        {
            SetActiveNodeContainer(containerController);
        };

        containerController.NodeAdditionRequested += (position) =>
        {
            if (activeNodeContainer != containerController)
            {
                Debug.LogWarning("Node addition requested by inactive container");
            }

            var node = containerController.TryAddingNodeAtPoint(position);
            if (node is null)
            {
                Debug.LogError($"Failed to add point add position {position}");
            }
        };

        containerController.ContainerSelected += () =>
        {
            SetActiveNodeContainer(containerController);
        };

        containerController.ContainerDeletionRequested += () =>
        {
            if (activeNodeContainer == containerController)
            {
                SetActiveNodeContainer(null);
            }
            containerController.Delete();
        };

        SetActiveNodeContainer(containerController);
        container.SetActive(true);

        return containerController;
    }
}

#nullable enable
using UnityEngine;
using Assets.Prefabs.MapBuilder;

[RequireComponent(typeof(IInputInformation))]
class NodeManager : MonoBehaviour, INodeManager
{
    private INodeContainer? _activeNodeContainer;
    private INodeContainer? ActiveNodeContainer
    {
        get => _activeNodeContainer;
        set
        {
            if (_activeNodeContainer == value)
            {
                return;
            }

            _activeNodeContainer?.ResetActivityState();
            _activeNodeContainer = value;
            _activeNodeContainer?.SelectContainer();
            SelectedContainerChanged?.Invoke(_activeNodeContainer);
        }
    }

    [SerializeField] public float addingPointThreshold = 1f;

    public event SelectedContainerChangedHandler? SelectedContainerChanged;

    public void ResetActivityState()
    {
        ActiveNodeContainer = null;
    }

    public void HandleVoidWasClicked(Vector3 position)
    {
        if (ActiveNodeContainer is null)
        {
            return;
        }

        var mousePos = position;
        var closestOnCollider = ActiveNodeContainer.GetClosestPointOnCollider(mousePos);
        Debug.Log($"Closest on collider: {closestOnCollider}");
        if (Vector2.Distance(mousePos, closestOnCollider) > addingPointThreshold)
        {
            ActiveNodeContainer = null;
            return;
        }

        var newNodeController = ActiveNodeContainer.TryAddingNodeAtPoint(closestOnCollider);

        if (newNodeController == null)
        {
            Debug.Log($"Failed to add point to container: {ActiveNodeContainer}");
            return;
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
            ActiveNodeContainer = containerController;
        };

        containerController.NodeAdditionRequested += (position) =>
        {
            if (ActiveNodeContainer != containerController)
            {
                Debug.LogWarning("Node addition requested by inactive container");
            }

            var node = containerController.TryAddingNodeAtPoint(position);
            if (node is null)
            {
                Debug.Log($"Failed to add point add position {position}");
            }
        };

        containerController.ContainerSelected += () =>
        {
            ActiveNodeContainer = containerController;
        };

        containerController.ContainerDeletionRequested += () =>
        {
            if (ActiveNodeContainer == containerController)
            {
                ActiveNodeContainer = null;
            }
            containerController.Delete();
        };

        ActiveNodeContainer = containerController;
        container.SetActive(true);

        return containerController;
    }

    public void ApplyTransformation(IContainerStateTransformation transformation)
    {
        ActiveNodeContainer?.ApplyTransformation(transformation);
    }
}

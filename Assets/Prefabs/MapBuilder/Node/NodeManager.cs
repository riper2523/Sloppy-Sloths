#nullable enable
using UnityEngine;
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Serialization;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

[System.Serializable]
public class OrdinaryNodeManagerDTO : INodeManagerDTO
{
    public NodeManagerType Type => NodeManagerType.ORDINARY;
    public List<INodeContainerDTO> NodeContainerDTOs { get; set; } = new();

    public OrdinaryNodeManagerDTO() { }
    public OrdinaryNodeManagerDTO(List<INodeContainerDTO> containers)
    {
        NodeContainerDTOs = containers;
    }

    public string GetPayload() => JsonConvert.SerializeObject(this);
}

[RequireComponent(typeof(IInputInformation))]
class NodeManager : MonoBehaviour, INodeManager
{
    private readonly List<INodeContainer> NodeContainers = new();

    [SerializeField] private DTOInstantiator? instantiator;

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

    public void DeleteActiveContainer()
    {
        if (ActiveNodeContainer != null)
        {
            var container = ActiveNodeContainer;
            ActiveNodeContainer = null;
            container.Delete();
            NodeContainers.Remove(container);
        }
    }

    public void HandleVoidWasClicked(Vector3 position)
    {
        if (ActiveNodeContainer is null)
        {
            Debug.Log("Active is null");
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
        var containerInstance = Instantiate(nodeContainerPrefab, position, Quaternion.identity, transform);
        var containerController = containerInstance.GetComponent<INodeContainer>();
        if (containerController is null)
        {
            Debug.LogError($"Prefab '{nodeContainerPrefab.name}' is missing a component implementing INodeContainer (for example, PolygonBuilder).", containerInstance);
            Destroy(containerInstance);
            return null;
        }

        InitializeContainer(containerController);
        containerInstance.SetActive(true);
        return containerController;
    }

    private void InitializeContainer(INodeContainer containerController)
    {
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
            NodeContainers.Remove(containerController);
        };

        ActiveNodeContainer = containerController;
        NodeContainers.Add(containerController);
    }

    public void ApplyTransformation(IContainerStateTransformation transformation)
    {
        ActiveNodeContainer?.ApplyTransformation(transformation);
    }

    public void Clear()
    {
        foreach (var container in NodeContainers.ToList())
        {
            container.Delete();
        }
        NodeContainers.Clear();
        ActiveNodeContainer = null;
    }

    public void MoveToGameplay()
    {
        foreach (var container in NodeContainers)
        {
            container.MoveToGameplay();
        }
    }

    public INodeManagerDTO SerializeToDTO()
    {
        var containerDTOs = NodeContainers.Select(c => c.SerializeToDTO()).ToList();
        return new OrdinaryNodeManagerDTO(containerDTOs);
    }

    public void SetUpUsingDTO(INodeManagerDTO dto)
    {
        // Clear existing containers
        foreach (var container in NodeContainers.ToList())
        {
            container.Delete();
        }
        NodeContainers.Clear();
        ActiveNodeContainer = null;

        if (instantiator == null)
        {
            Debug.LogError("NodeManager: DTOInstantiator is not assigned. Cannot deserialize containers.");
            return;
        }

        foreach (var containerDTO in dto.NodeContainerDTOs)
        {
            var container = instantiator.InstantiateContainer(containerDTO, transform);
            if (container != null)
            {
                InitializeContainer(container);
            }
        }
    }

    public string SerializeToJson()
    {
        var dto = SerializeToDTO();

        return JsonConvert.SerializeObject(dto, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        });
    }
}

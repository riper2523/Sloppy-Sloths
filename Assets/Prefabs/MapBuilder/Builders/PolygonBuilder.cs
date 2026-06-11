using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Assets.Prefabs.MapBuilder;
using UnityEngine.EventSystems;
using Assets.Prefabs.MapBuilder.Node;
using Assets.Prefabs.MapBuilder.Serialization;

[System.Serializable]
public class PolygonBuilderDTO : INodeContainerDTO
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public NodeContainerType Type => NodeContainerType.POLYGON;
    public List<INodeHandleDTO> NodeHandleDTOs { get; set; } = new();

    public PolygonBuilderDTO() { }
    public PolygonBuilderDTO(Vector3 pos, List<INodeHandleDTO> nodes)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
        NodeHandleDTOs = nodes;
    }

    public Vector3 AsVector3() => new Vector3(x, y, z);
}

[RequireComponent(typeof(SpriteShapeController))]
[RequireComponent(typeof(SpriteShapeRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonBuilder : MonoBehaviour, INodeContainer, IPointerUpHandler, IPointerDownHandler
{
    private IInputInformation inputInformation;
    private SpriteShapeController shape;
    private Spline spline;
    private Collider2D splineCollider;
    private SpriteShapeRenderer spriteRenderer;

    private readonly IList<INodeHandle> nodes = new List<INodeHandle>();

    private Color originalColor;

    [SerializeField]
    private GameObject nodePrefab;

    [SerializeField]
    public Color tintColor = Color.gray;

    [SerializeField]
    private float distanceThreshold = 0.05f;

#nullable enable
    public event NodeInContainerChangedSelectionState? NodeChangedState;
    public event NodeAdditionRequestedHandler? NodeAdditionRequested;
    public event Action? ContainerSelected;
    public event NodesInContainerDeletedHandler? NodesDeleted;
    public event Action? ContainerDeletionRequested;

    private bool ColliderNeedsRebuilding;

    private readonly NodesContainerActivityState ActivityState = NodesContainerActivityState.ContainerInactive();
    NodesContainerActivityState INodeContainer.ActivityState => ActivityState;

    public void ResetActivityState()
    {
        ActivityState.SetNewState(NodesContainerActivityState.ContainerInactive());
    }

    // Returns true if the deletion of the entire container is neccessary
    private void DeleteANode(INodeHandle node)
    {
        bool res = nodes.Remove(node);
        Debug.Assert(res, $"node: {node} not found in NodeContainer structures");
        ActivityState.RemoveFromActiveNodes(node);

        node.Delete();
    }

    public void OnNodeTrigger(INodeHandle nodeHandle)
    {
        Debug.Log($"Node {nodeHandle} triggered");
        if (!nodes.Contains(nodeHandle))
        {
            Debug.LogError($"Node: {nodeHandle} not found in NodeContainer: {this} structures");
            return;
        }

        if (inputInformation.IsCtrlPressed())
        {
            // If the node was not present in the active nodes then add it
            if (!ActivityState.RemoveFromActiveNodes(nodeHandle))
            {
                ActivityState.AddToActiveNodes(nodeHandle);
            }
        }
        else if (!ActivityState.IsTheNodeActive(nodeHandle))
        {
            ActivityState.SetNewState(NodesContainerActivityState.ContainerActiveWithOnlyOneNode(nodeHandle));
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        shape = GetComponent<SpriteShapeController>()!;

        spriteRenderer = GetComponent<SpriteShapeRenderer>()!;

        originalColor = spriteRenderer!.color;

        spline = shape!.spline;
        splineCollider = GetComponent<Collider2D>()!;

        inputInformation = GetComponentInParent<IInputInformation>()!;
        Debug.Assert(inputInformation is not null);
    }

    void Start()
    {
        // If nodes were already populated (e.g. loaded from DTO), do not add default spline points
        if (nodes.Count > 0) return;

        for (int i = 0; i < spline.GetPointCount(); i++)
        {
            var splinePoint = spline.GetPosition(i) + shape.transform.position;
            NodeAdditionRequested?.Invoke(splinePoint);
        }
    }

    void Update()
    {
        if (ColliderNeedsRebuilding)
        {
            RebuildTheCollider();
            ColliderNeedsRebuilding = false;
        }

        var nextColor = ActivityState.IsContainerActive() ? tintColor : originalColor;
        if (spriteRenderer.color != nextColor)
        {
            spriteRenderer.color = nextColor;
        }

        if (ActivityState.IsContainerActive() && inputInformation.DelKeyWasClicked())
        {
            var deletedNodes = ActivityState.ActiveNodes;
            var deletingContainer = false;

            if (!ActivityState.AreAnyNodesActive() || nodes.Count - deletedNodes.Count < 3)
            {
                deletedNodes = new HashSet<INodeHandle>(nodes);
                deletingContainer = true;
            }

            // This has to happen before the nodes deletion since it accesses INodeHandles which will be lost after deletion
            ActivityState.SetNewState(NodesContainerActivityState.ContainerActiveButNoNodesSelected());

            foreach (var node in deletedNodes)
            {
                Debug.Log($"Deleting node {node}");
                DeleteANode(node);
            }
            NodesDeleted?.Invoke(deletedNodes);

            if (deletingContainer)
            {
                ContainerDeletionRequested?.Invoke();
            }
            else
            {
                RebuildTheSplineAndCollider();
            }
        }
    }


    // OnPointerDown is necessary for OnPointerUp to function
    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"Was clicked, is active {ActivityState.IsContainerActive()}");
        if (!ActivityState.IsContainerActive())
        {
            ContainerSelected?.Invoke();
        }

        if (inputInformation.IsCtrlPressed())
        {
            foreach (var node in nodes)
            {
                ActivityState.AddToActiveNodes(node);
            }
        }
    }

    public Vector2 GetClosestPointOnCollider(Vector2 point)
    {
        return splineCollider.ClosestPoint(point);
    }

    private static DoubledAreaAndBaseLength GetProximityMetric(Vector2 a, Vector2 b, Vector2 other)
    {
        return new(Vector3.Cross((Vector3)(a - other), (Vector3)(b - other)).magnitude, Vector2.Distance(a, b));
    }

    private int? FindIndex(Vector2 position)
    {
        if (nodes.Count <= 1)
        {
            return nodes.Count;
        }

        int? answer = null;
        var b = nodes[0].Coordinates;

        var ourNodeCoords = position;

        var smallestVal = DoubledAreaAndBaseLength.MAX;

        for (int i = 1; i <= nodes.Count; i++)
        {
            var a = b;
            b = nodes[i % nodes.Count].Coordinates;

            if (GeometricUtils.ThereAreObtuseAnglesNearAB(a, b, ourNodeCoords))
            {
                continue;
            }

            var currVal = GetProximityMetric(a, b, ourNodeCoords);

            if (currVal.CompareTo(smallestVal) < 0)
            {
                smallestVal = currVal;
                answer = i;
            }
        }

        return answer;
    }

    private bool AddToSpline(INodeHandle nodeController, int index)
    {
        nodes.Insert(index, nodeController);
        return true;
    }

    private void RebuildTheSpline()
    {
        spline.Clear();
        for (int i = 0; i < nodes.Count; i++)
        {
            spline.InsertPointAt(i, (Vector2)nodes[i].Coordinates - (Vector2)shape.transform.position);
        }

        shape.RefreshSpriteShape();
    }

    private void RebuildTheCollider()
    {
        shape.BakeCollider();
    }

    private void RebuildTheSplineAndCollider()
    {
        RebuildTheSpline();
        RebuildTheCollider();
    }

    public void NodeMoved(INodeHandle node, Vector2 offset)
    {
        if (ActivityState.IsTheNodeActive(node))
        {
            foreach (var otherNode in ActivityState.ActiveNodes)
            {
                if (otherNode != node)
                {
                    otherNode.MoveByOffset(offset);
                }
            }
        }
        RebuildTheSpline();
    }


    public (INodeHandle, GameObject)? CreateNode(Vector2 position)
    {
        GameObject newClone = Instantiate(nodePrefab, position, Quaternion.identity, transform);
        var controller = newClone.GetComponent<INodeHandle>();
        if (controller is null)
        {
            Debug.LogError($"Prefab '{nodePrefab.name}' is missing NodeController.", newClone);
            Destroy(newClone);
            return null;
        }

        return (controller, newClone);
    }

    public INodeHandle? TryAddingNodeAtPoint(Vector2 position)
    {
        var minDistance = nodes.Select(x => Vector2.Distance(position, x.Coordinates)).DefaultIfEmpty(float.PositiveInfinity).Min();
        if (minDistance < distanceThreshold)
        {
            Debug.Log($"There is a node which is {minDistance} close to the node you are trying to add");
            return null;
        }

        var nodeIndex = FindIndex(position);
        if (nodeIndex is null)
        {
            Debug.Log("Addition would cause the creation of obtuse angles");
            return null;
        }

        var nodeData = CreateNode(position);
        if (nodeData is null)
        {
            Debug.LogError("Couldn't create the node");
            return null;
        }

        (var controller, _) = nodeData.Value;

        controller.NodeTriggered += () => OnNodeTrigger(controller);

        controller.NodeDragged += (_, offset) => NodeMoved(controller, offset);

        controller.NodeChangedSelectionState += () => NodeChangedState?.Invoke(controller);

        controller.NodeDragEnded += () =>
        {
            ColliderNeedsRebuilding = true;
        };

        var result = AddToSpline(controller, nodeIndex.Value);

        if (nodes.Count >= 3)
        {
            RebuildTheSplineAndCollider();
        }
        return controller;
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    public void SelectContainer()
    {
        ActivityState.SetNewState(NodesContainerActivityState.ContainerActiveButNoNodesSelected());
    }

    public NodeContainerState GetNodeContainerStateCopy()
    {
        return new NodeContainerState(nodes.Select(x => x.Coordinates).ToList());
    }

    public void ApplyTransformation(IContainerStateTransformation transformation)
    {
        var state = GetNodeContainerStateCopy();
        transformation.TransformInPlace(state);

        Debug.Assert(nodes.Count == state.Nodes.Count);
        for (var i = 0; i < nodes.Count; i++)
        {
            nodes[i].Coordinates = state.Nodes[i];
        }

        RebuildTheSplineAndCollider();
    }

    public INodeContainerDTO SerializeToDTO()
    {
        var nodeDTOs = nodes.Select(n => n.SerializeToDTO()).ToList();
        return new PolygonBuilderDTO(transform.position, nodeDTOs);
    }

    public void SetUpUsingDTO(INodeContainerDTO nodeContainerDTO)
    {
        if (nodeContainerDTO is PolygonBuilderDTO polygonDTO)
        {
            transform.position = polygonDTO.AsVector3();

            // Clear existing nodes (like those created in Start from the initial spline)
            foreach (var node in nodes.ToList())
            {
                DeleteANode(node);
            }
            nodes.Clear();
            spline.Clear();

            foreach (var nodeDTO in polygonDTO.NodeHandleDTOs)
            {
                // We use world coordinates from the DTO
                var nodeData = CreateNode(nodeDTO is NodeControllerDTO nDTO ? nDTO.AsVector3() : Vector3.zero);
                if (nodeData != null)
                {
                    var (controller, _) = nodeData.Value;

                    // Hook up events similar to TryAddingNodeAtPoint
                    controller.NodeTriggered += () => OnNodeTrigger(controller);
                    controller.NodeDragged += (_, offset) => NodeMoved(controller, offset);
                    controller.NodeChangedSelectionState += () => NodeChangedState?.Invoke(controller);
                    controller.NodeDragEnded += () => ColliderNeedsRebuilding = true;

                    nodes.Add(controller);
                }
            }

            RebuildTheSplineAndCollider();
        }
    }

    public void MoveToGameplay()
    {
        foreach (var node in nodes)
        {
            node.Delete();
        }
        nodes.Clear();
    }
}

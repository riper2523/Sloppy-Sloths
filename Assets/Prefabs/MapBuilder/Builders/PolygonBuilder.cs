using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Node;
using UnityEngine.EventSystems;

//TODO: Check how are GameObjects identified, read about
// - **Instance ID** (runtime, unique per session):  
//   `gameObject.GetInstanceID()` gives an integer unique while the app/editor session runs. Comparing these is equivalent to comparing triangle heights
class DoubledAreaAndBaseLength : IComparable<DoubledAreaAndBaseLength>
{
    private readonly float doubleArea;
    private readonly float baseLen;

    public DoubledAreaAndBaseLength(float doubleArea, float baseLen)
    {
        this.doubleArea = doubleArea;
        this.baseLen = baseLen;
    }

    //Check if the height is bigger
    public int CompareTo(DoubledAreaAndBaseLength other)
    {
        if (other is null)
        {
            Debug.LogWarning("This shouldn't happen");
            return 1;
        }
        return (other.baseLen * doubleArea).CompareTo(baseLen * other.doubleArea);
    }
}

public class PolygonBuilder : MonoBehaviour, INodeContainer, IPointerClickHandler
{
    NodesContainerActivityState ActivityState = NodesContainerActivityState.ContainerInactive;
    NodesContainerActivityState INodeContainer.ActivityState => ActivityState;

    private SpriteShapeController shape;
    private Spline spline;
    private Camera mainCam;
    private Collider2D splineCollider;

    private readonly List<INodeHandle> nodes = new();

    [SerializeField]
    private GameObject nodePrefab;

    public INodeManager nodeManager;

    private Color originalColor;

    [SerializeField]
    public Color tintColor = Color.gray;


    private SpriteShapeRenderer spriteRenderer;

    public event NodeInContainerTriggeredHandler NodeSelected;
    public event NodeAdditionRequestedHandler NodeAdditionRequested;
    public event Action ContainerSelected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        shape = GetComponent<SpriteShapeController>();
        spriteRenderer = GetComponent<SpriteShapeRenderer>();
        originalColor = spriteRenderer.color;

        spline = shape.spline;
        mainCam = Camera.main;
        splineCollider = GetComponent<Collider2D>();
        Debug.Log("Awake");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Was clicked, is active {ActivityState.IsContainerActive()}");
        if (!ActivityState.IsContainerActive())
        {
            ContainerSelected?.Invoke();
        }
    }

    void Start()
    {
        Debug.Log("Started the NodeContainer");
        for (int i = 0; i < spline.GetPointCount(); i++)
        {
            var splinePoint = spline.GetPosition(i) + shape.transform.position;
            NodeAdditionRequested?.Invoke(this, splinePoint);
        }
    }

    void Update()
    {
        var nextColor = ActivityState.IsContainerActive() ? tintColor : originalColor;
        if (spriteRenderer.color != nextColor)
        {
            spriteRenderer.color = nextColor;
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

    private static bool ThereAreObtuseAnglesNearAB(Vector2 A, Vector2 B, Vector2 other)
    {
        if (Vector2.Dot(A - B, other - B) < 0 || Vector2.Dot(B - A, other - A) < 0)
        {
            return true;
        }

        return false;
    }

    private int FindIndex(INodeHandle nodeController)
    {
        if (nodes.Count <= 1)
        {
            return nodes.Count;
        }

        var answer = 1;
        var a = nodes[0].GetCoordinates();
        var b = nodes[1].GetCoordinates();
        var ourNodeCoords = nodeController.GetCoordinates();
        var smallestVal = GetProximityMetric(a, b, ourNodeCoords);

        for (int i = 2; i <= nodes.Count; i++)
        {
            a = b;
            b = nodes[i % nodes.Count].GetCoordinates();

            if (ThereAreObtuseAnglesNearAB(a, b, ourNodeCoords))
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

    private void AddToSpline(INodeHandle nodeController)
    {
        //TODO: check whether the attached script objects change during the runtime
        Debug.Log($"Adding to spline: {nodeController.GetCoordinates()}");
        nodes.Insert(FindIndex(nodeController), nodeController);
    }

    private void RebuildTheSpline()
    {
        spline.Clear();
        for (int i = 0; i < nodes.Count; i++)
        {
            spline.InsertPointAt(i, (Vector2)nodes[i].GetCoordinates() - (Vector2)shape.transform.position);
        }

        Debug.Log($"Rebuilt the spline, new spline is {spline.GetPointCount()}");

        shape.RefreshSpriteShape();
        shape.BakeCollider();
    }

    private void RemoveFromSpline(INodeHandle nodeController)
    {
        nodes.Remove(nodeController);
        RebuildTheSpline();
        Debug.Log($"Removing from spline: {nodeController.GetCoordinates()}");
    }

    public void NodeMoved(INodeHandle _)
    {
        RebuildTheSpline();
    }

    public bool DeletePrimaryNode()
    {
        if (nodes.Count <= 3)
        {
            return false;
        }

        if (ActivityState.IsContainerActive() == false)
        {
            Debug.LogError("Trying to delete a node from an inactive container");
            return false;
        }

        if (ActivityState.ActiveNodes.Count == 0)
        {
            Debug.LogError("No primary node selected");
            return false;
        }

        var node = ActivityState.ActiveNodes[0];
        if (!nodes.Remove(node))
        {
            Debug.LogError($"Node: {node} not found in NodesContainer: {this} structures");
            return false;
        }

        node.Delete();
        ActivityState.ActiveNodes.Remove(node);
        RebuildTheSpline();
        return true;
    }

    public INodeHandle CreateNode(Vector2 position)
    {
        GameObject newClone = Instantiate(nodePrefab, position, Quaternion.identity, transform);
        var controller = newClone.GetComponent<INodeHandle>();
        if (controller is null)
        {
            Debug.LogError($"Prefab '{nodePrefab.name}' is missing NodeController.", newClone);
            Destroy(newClone);
            return null;
        }

        return controller;
    }

    public INodeHandle TryAddingNodeAtPoint(Vector2 position)
    {
        var controller = CreateNode(position);
        if (controller is null)
        {
            Debug.LogError("The addition was not succesfull");
            return null;
        }

        controller.NodeSelected += controller => NodeSelected?.Invoke(this, controller);
        controller.NodeMoved += _ => RebuildTheSpline();

        AddToSpline(controller);
        if (nodes.Count >= 3)
        {
            RebuildTheSpline();
        }
        return controller;
    }

    public void HandleNodeSelection(INodeHandle nodeHandle)
    {
        if (!nodes.Contains(nodeHandle))
        {
            Debug.LogError($"Node: {nodeHandle} not found in NodeContainer: {this} structures");
            return;
        }

        if (!ActivityState.IsContainerActive())
        {
            ActivityState = new NodesContainerActivityState(new() { nodeHandle });
        }
        else if (!nodeHandle.Active)
        {
            ActivityState.ActiveNodes.Add(nodeHandle);
        }
        nodeHandle.Active = true;
    }

    public void ResetActivityState()
    {
        if (ActivityState.ActiveNodes != null)
        {
            foreach (var node in ActivityState.ActiveNodes)
            {
                node.Active = false;
            }
        }
        ActivityState = NodesContainerActivityState.ContainerInactive;
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    public void HandleContainerSelection()
    {
        ActivityState = NodesContainerActivityState.ContainerActiveButNoNodesSelected;
    }
}

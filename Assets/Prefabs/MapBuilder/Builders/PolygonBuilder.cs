using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Node;

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

public class PolygonBuilder : MonoBehaviour, INodeContainer
{
    private SpriteShapeController shape;
    private Spline spline;
    private Camera mainCam;
    private Collider2D splineCollider;

    private readonly List<INodeHandle> nodes = new();


    [SerializeField] private GameObject nodeManagerSource;

    public INodeManager nodeManager;

    [SerializeField]
    public float addingPointThreeshold = 1f;

    public IInputInformation inputInformation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        nodeManager = nodeManagerSource.GetComponent<INodeManager>();
        inputInformation = nodeManagerSource.GetComponent<IInputInformation>();

        if (nodeManager == null)
            Debug.LogError("The assigned object does not implement INodeManager!");

        if (inputInformation == null)
            Debug.LogError("The assigned object does not implement IInputInformation!");


        shape = GetComponent<SpriteShapeController>();
        spline = shape.spline;
        mainCam = Camera.main;
        splineCollider = GetComponent<Collider2D>();
        Debug.Log("Awake");
    }

    void Start()
    {
        for (int i = 0; i < spline.GetPointCount(); i++)
        {
            var splinePoint = spline.GetPosition(i) + shape.transform.position;
            var controller = nodeManager.TryAddingNodeAtPoint(splinePoint);
            if (controller == null)
            {
                Debug.LogError($"This controller shouldn't be null!!! index: {i}");
            }
            else
            {
                AddToSpline(controller);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = inputInformation.GetMouseWorldPos();
        var closestOnCollider = GetClosestPointOnCollider(mousePos);

        if (inputInformation.WeClickedThisFrame())
        {
            Debug.Log($"Closest on collider: {closestOnCollider}");
            if (Vector2.Distance(mousePos, closestOnCollider) > addingPointThreeshold || nodeManager.GetActiveNode() != null)
            {
                return;
            }

            //TODO: Prevent the symoultaneous addition and selection, prevent addition of more than one node at a time
            var newNodeController = nodeManager.TryAddingNodeAtPoint(closestOnCollider);

            if (newNodeController == null)
            {
                return;
            }

            AddToSplineAndRebuild(newNodeController);
        }
    }

    private Vector2 GetClosestPointOnCollider(Vector2 point)
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
        //TODO: change this
        nodeController.SetTheNodeUp(this, inputInformation);
        nodeController.Active = true;
        nodes.Insert(FindIndex(nodeController), nodeController);
    }

    private void AddToSplineAndRebuild(INodeHandle nodeController)
    {
        AddToSpline(nodeController);
        RebuildTheSpline();
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

    bool INodeContainer.TryDeletingThis(INodeHandle node)
    {
        if (nodes.Count <= 3 || nodeManager.TryDelete(node) == false)
        {
            return false;
        }

        if (!nodes.Remove(node))
        {
            Debug.LogError($"Trying to remove node that is not present the PolygonBuilder structures: {node}");
            return false;
        }
        RemoveFromSpline(node);
        return true;
    }

    public bool CanNodeMove(INodeHandle node)
    {
        return IsThisNodeActive(node);
    }

    public bool IsThisNodeActive(INodeHandle node)
    {
        return nodeManager.GetActiveNode() == node;
    }

    public bool TryActivatingTheNode(INodeHandle node)
    {
        return nodeManager.TryActivatingNode(node);
    }

    public void NodeMoved(INodeHandle _)
    {
        RebuildTheSpline();
    }
}

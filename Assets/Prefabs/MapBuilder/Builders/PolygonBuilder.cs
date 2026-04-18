using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Assets.Prefabs.MapBuilder.Node;
using UnityEngine.InputSystem;

//TODO: Check how are GameObjects identified, read about
// - **Instance ID** (runtime, unique per session):  
//   `gameObject.GetInstanceID()` gives an integer unique while the app/editor session runs.
//
public class PolygonBuilder : MonoBehaviour
{
  private SpriteShapeController shape;
  private Spline spline;
  private Camera mainCam;
  private Collider2D splineCollider;

  private readonly List<NodeController> nodes = new();

  [SerializeField]
  private NodeManager nodeManager;

  [SerializeField]
  public float addingPointThreeshold = 1f;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    shape = GetComponent<SpriteShapeController>();
    spline = shape.spline;
    mainCam = Camera.main;
    splineCollider = GetComponent<Collider2D>();
    Debug.Log("Started");

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
        addToSpline(controller);
      }
    }
  }

  // Update is called once per frame
  void Update()
  {
    Vector2 mousePos = GetMouseWorldPos();

    var closestOnCollider = GetClosestPointOnCollider(mousePos);

    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
    {
      Debug.Log($"Closest on collider: {closestOnCollider}");
      if (Vector2.Distance(mousePos, closestOnCollider) > addingPointThreeshold || nodeManager.getActiveNode() != null)
      {
        return;
      }

      //TODO: Prevent the symoultaneous addition and selection, prevent addition of more than one node at a time
      var newNodeController = nodeManager.TryAddingNodeAtPoint(closestOnCollider);
      if (newNodeController == null)
      {
        return;
      }

      addToSplineAndRebuild(newNodeController);
    }
  }

  private Vector3 GetMouseWorldPos()
  {
    Vector2 mousePos2D = Mouse.current.position.ReadValue();

    Vector3 mousePos3D = new(mousePos2D.x, mousePos2D.y, 0);
    return mainCam.ScreenToWorldPoint(mousePos3D);
  }

  private Vector2 GetClosestPointOnCollider(Vector2 point)
  {
    return splineCollider.ClosestPoint(point);
  }

  private void addToSpline(NodeController nodeController)
  {
    //TODO: check whether the attached script objects change during the runtime
    Debug.Log($"Adding to spline: {nodeController.getCoordinates()}");
    //TODO: change this
    nodes.Add(nodeController);

    nodeController.NodeDeleted += (_, _) => removeFromSpline(nodeController); nodeController.NodeMoved += (_, _) => removeFromSpline(nodeController);
  }

  private void addToSplineAndRebuild(NodeController nodeController, bool withRebuild = true)
  {
    addToSpline(nodeController);

    if (withRebuild)
    {
      rebuildTheSpline();
    }
  }

  private void rebuildTheSpline()
  {

    spline.Clear();
    for (int i = 0; i < nodes.Count; i++)
    {
      spline.InsertPointAt(i, nodes[i].getCoordinates() - shape.transform.position);
    }

    Debug.Log($"Rebuilt the spline, new spline is {spline.GetPointCount()}");

    shape.RefreshSpriteShape();
    shape.BakeCollider();
  }

  private void removeFromSpline(NodeController nodeController)
  {
    nodes.Remove(nodeController);
    rebuildTheSpline();
    Debug.Log($"Removing from spline: {nodeController.getCoordinates()}");
  }
}

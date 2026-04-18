using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Prefabs.MapBuilder.Node
{
  class NodeManager : MonoBehaviour
  {
    private NodeController activeNode;
    private Camera mainCam;
    private readonly List<NodeController> nodes = new();

    [SerializeField] public float additionThreeshold = 0.5f;

    [SerializeField] private GameObject myPrefab;

    public NodeController getActiveNode()
    {
      return activeNode;
    }

    public NodeController TryAddingNodeAtPoint(Vector2 point)
    {
      Debug.Log($"Trying to add a point: {point}");
      Vector3 spawnPos = new(point.x, point.y, 0);

      if (myPrefab == null)
      {
        Debug.LogError("NodeManager.myPrefab is not assigned.");
        return null;
      }

      if (nodes.Find(controller => { return Vector2.Distance(controller.getCoordinates(), point) < additionThreeshold; })
      != null)
      {
        Debug.Log("There already is a point which is really close to that position");
        return null;
      }

      GameObject newClone = Instantiate(myPrefab, spawnPos, Quaternion.identity);

      var controller = newClone.GetComponent<NodeController>();
      if (controller == null)
      {
        Debug.LogError($"Prefab '{myPrefab.name}' is missing NodeController.", newClone);
        Destroy(newClone);
        return null;
      }

      nodes.Add(controller);
      activeNode = controller;

      controller.NodeActivated += (sender, e) => { activeNode = controller; };
      controller.NodeDeleted += (sender, e) => nodes.Remove(controller);

      Debug.Log($"Point instantiated, its position: {newClone.transform.position}");

      return controller;
    }

    void Start()
    {
      mainCam = Camera.main;
    }

    void Update()
    {
      var mousePos = GetMouseWorldPos();
      if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && activeNode != null)
      {
        if (!activeNode.doesCollide(mousePos))
        {
          activeNode = null;
        }
      }

      if (Keyboard.current != null && Keyboard.current.deleteKey.wasPressedThisFrame && activeNode != null)
      {
        activeNode.deleteNode();
        activeNode = null;
      }
    }

    private Vector3 GetMouseWorldPos()
    {
      Vector2 mousePos2D = Mouse.current.position.ReadValue();

      Vector3 mousePos3D = new(mousePos2D.x, mousePos2D.y, 0);
      return mainCam.ScreenToWorldPoint(mousePos3D);
    }
  }
}

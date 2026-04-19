using System.Collections.Generic;
using System;
using UnityEngine;
using Assets.Prefabs.MapBuilder;

class NodeManager : MonoBehaviour, INodeManager
{
    private INodeHandle activeNode;
    private Camera mainCam;
    private readonly List<INodeHandle> nodes = new();

    [SerializeField] public float additionThreeshold = 0.5f;

    [SerializeField] private GameObject myPrefab;

    //TODO: think about making this a [SerializeField]
    private IInputInformation inputInformation;

    void Start()
    {
        inputInformation = GetComponent<IInputInformation>();
    }

    public INodeHandle GetActiveNode()
    {
        return activeNode;
    }

    public INodeHandle TryAddingNodeAtPoint(Vector2 point)
    {
        Debug.Log($"Trying to add a point: {point}");
        Vector3 spawnPos = new(point.x, point.y, 0);

        if (myPrefab == null)
        {
            Debug.LogError("NodeManager.myPrefab is not assigned.");
            return null;
        }

        if (nodes.Find(controller => { return Vector2.Distance(controller.GetCoordinates(), point) < additionThreeshold; })
        != null)
        {
            Debug.Log("There already is a point which is really close to that position");
            return null;
        }

        GameObject newClone = Instantiate(myPrefab, spawnPos, Quaternion.identity);

        var controller = newClone.GetComponent<INodeHandle>();
        if (controller == null)
        {
            Debug.LogError($"Prefab '{myPrefab.name}' is missing NodeController.", newClone);
            Destroy(newClone);
            return null;
        }

        nodes.Add(controller);
        activeNode = controller;

        Debug.Log($"Point instantiated, its position: {newClone.transform.position}");

        return controller;
    }

    void Update()
    {
        if (inputInformation.WeClickedThisFrame() && activeNode != null)
        {
            var mousePos = inputInformation.GetMouseWorldPos();
            if (!activeNode.DoesCollide(mousePos))
            {
                activeNode = null;
            }
        }
    }

    public bool TryDelete(INodeHandle node)
    {
        if (!nodes.Remove(node))
        {
            Debug.LogError($"Trying to delete a node that is not present in the NodeManager structures: {node}");
            return false;
        }
        if (activeNode == node)
        {
            activeNode = null;
        }
        return true;
    }

    public bool TryActivatingNode(INodeHandle node)
    {
        if (!nodes.Contains(node))
        {
            Debug.LogError($"Trying to activate a node that is not present in the NodeManager structures: {node}");
            return false;
        }

        Debug.Log($"Activated the node: {node}");
        if (activeNode != null)
        {
            activeNode.Active = false;
        }
        activeNode = node;

        return true;
    }
}

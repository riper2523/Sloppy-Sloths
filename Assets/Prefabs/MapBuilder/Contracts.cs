using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Prefabs.MapBuilder
{
    public delegate void NodeTriggeredHandler(INodeHandle node);
    public interface INodeHandle
    { // This is triggered for example when clicking the node
        public event NodeTriggeredHandler? NodeSelected;
        public event NodeTriggeredHandler? NodeMoved;

        Vector3 GetCoordinates();
        void Delete();
        bool Active { get; set; }
    }


    // You can select multiple nodes and then move them together, 
    // you can only select the container and no nodes in it - in that case the list is empty
    // when container is inactive then this value is equal to ContainerInactive
    public class NodesContainerActivityState
    {
        public List<INodeHandle> ActiveNodes { get; }

        public NodesContainerActivityState(List<INodeHandle> activeNodes)
        {
            ActiveNodes = activeNodes;
        }

        //Exists only to initialize inactive container
        private NodesContainerActivityState()
        {
            ActiveNodes = null;
        }

        public static NodesContainerActivityState ContainerInactive { get; } = new();
        public static NodesContainerActivityState ContainerActiveButNoNodesSelected { get; } = new(new() { });

        public bool IsContainerActive() { return !(this == ContainerInactive); }

        public bool AreAnyNodesActive()
        {
            return IsContainerActive() && ActiveNodes.Count > 0;
        }
    }

    public delegate void NodeInContainerTriggeredHandler(INodeContainer nodeContainer, INodeHandle nodeHandle);
    public delegate void NodeAdditionRequestedHandler(INodeContainer nodeContainer, Vector2 position);

    //TODO: create a wrapper objects that has executes these methods only on a certain node
    public interface INodeContainer
    {
        // This is to pass the signal that a node triggered to the node manager
        public event NodeInContainerTriggeredHandler? NodeSelected;
        public event Action? ContainerSelected;
        public event NodeAdditionRequestedHandler? NodeAdditionRequested;

        bool DeletePrimaryNode();
        INodeHandle TryAddingNodeAtPoint(Vector2 point);

        void Delete();

        // NodeManager triggers it to make the container update its structures
        void HandleNodeSelection(INodeHandle nodeHandle);
        void HandleContainerSelection();
        void ResetActivityState();

        Vector2 GetClosestPointOnCollider(Vector2 point);

        NodesContainerActivityState ActivityState { get; }
    }

    public interface INodeManager
    {
        INodeContainer AddNodeContainerAtPos(GameObject nodeContainerPrefab, Vector2 position);
    }


    public interface IInputInformation
    {
        Vector3 GetMouseWorldPos();
        bool WeClickedThisFrame();
        bool DelKeyWasClicked();
        bool EscapeKeyWasClicked();
        bool IsCtrlPressed();
        bool VoidWasClicked();
    }
}

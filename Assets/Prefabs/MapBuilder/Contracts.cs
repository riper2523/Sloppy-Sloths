#nullable enable
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Prefabs.MapBuilder
{
    public delegate void NodeDraggedHandler(Vector3 actPos, Vector3 offset);

    public interface INodeHandle
    {
        public event Action? NodeChangedSelectionState;
        // This is triggered for example when clicking the node
        public event Action? NodeTriggered;
        public event NodeDraggedHandler? NodeDragged;
        public event Action? NodeDragEnded;

        Vector3 GetCoordinates();
        void MoveByOffset(Vector3 offset);
        void Delete();
        bool Active { get; set; }
    }


    // You can select multiple nodes and then move them together, 
    // you can only select the container and no nodes in it - in that case the list is empty
    // when container is inactive then this value is equal to ContainerInactive
    public delegate void NodeInContainerChangedSelectionState(INodeHandle nodeHandle);
    public delegate void NodesInContainerDeletedHandler(ISet<INodeHandle> nodeHandles);
    public delegate void NodeAdditionRequestedHandler(Vector2 position);

    public interface INodeContainer
    {
        public event NodeInContainerChangedSelectionState? NodeChangedState;
        public event NodesInContainerDeletedHandler? NodesDeleted;
        public event Action? ContainerSelected;
        public event Action? ContainerDeleted;
        public event NodeAdditionRequestedHandler? NodeAdditionRequested;

        INodeHandle? TryAddingNodeAtPoint(Vector2 point);

        void Delete();

        // NodeManager triggers it to make the container update its structures
        void ResetActivityState();
        void SelectContainer();
        Vector2 GetClosestPointOnCollider(Vector2 point);

        NodesContainerActivityState ActivityState { get; }
    }

    public interface INodeManager
    {
        INodeContainer? AddNodeContainerAtPos(GameObject nodeContainerPrefab, Vector3 position);
    }


    public interface IInputInformation
    {
        Vector3 GetMouseWorldPos();
        bool WeReleasedThisFrame();
        bool WeClickedThisFrame();
        bool DelKeyWasClicked();
        bool EscapeKeyWasClicked();
        bool IsCtrlPressed();
        bool VoidWasClicked();
    }
}

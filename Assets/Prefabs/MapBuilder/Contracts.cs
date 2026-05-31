#nullable enable
using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Prefabs.MapBuilder.Serialization;

namespace Assets.Prefabs.MapBuilder
{
    public delegate void NodeDraggedHandler(Vector3 actPos, Vector3 offset);

    public interface ISerializableToDTO<T>
    {
        T SerializeToDTO();
        void SetUpUsingDTO(T dto);
    }

    public interface INodeHandle : ISerializableToDTO<INodeHandleDTO>
    {
        public event Action? NodeChangedSelectionState;
        // This is triggered for example when clicking the node
        public event Action? NodeTriggered;
        public event NodeDraggedHandler? NodeDragged;
        public event Action? NodeDragEnded;

        Vector3 Coordinates { get; set; }
        void MoveByOffset(Vector3 offset)
        {
            Coordinates = Coordinates + offset;
        }
        void Delete();
        bool Active { get; set; }
    }


    // You can select multiple nodes and then move them together, 
    // you can only select the container and no nodes in it - in that case the list is empty
    // when container is inactive then this value is equal to ContainerInactive
    public delegate void NodeInContainerChangedSelectionState(INodeHandle nodeHandle);
    public delegate void NodesInContainerDeletedHandler(ISet<INodeHandle> nodeHandles);
    public delegate void NodeAdditionRequestedHandler(Vector2 position);

    public class NodeContainerState
    {
        public List<Vector3> Nodes { get; }

        public NodeContainerState(List<Vector3> nodes)
        {
            Nodes = new(nodes);
        }

        public NodeContainerState()
        {
            Nodes = new();
        }
    }

    public interface IContainerStateTransformation
    {
        void TransformInPlace(NodeContainerState state);
    }

    public interface INodeContainer : ISerializableToDTO<INodeContainerDTO>
    {
        public event NodeInContainerChangedSelectionState? NodeChangedState;
        public event NodesInContainerDeletedHandler? NodesDeleted;
        public event Action? ContainerSelected;
        public event Action? ContainerDeletionRequested;
        public event NodeAdditionRequestedHandler? NodeAdditionRequested;

        public NodeContainerState GetNodeContainerStateCopy();
        public void ApplyTransformation(IContainerStateTransformation transformation);

        INodeHandle? TryAddingNodeAtPoint(Vector2 point);

        void Delete();

        // NodeManager triggers it to make the container update its structures
        void ResetActivityState();
        void SelectContainer();
        Vector2 GetClosestPointOnCollider(Vector2 point);

        NodesContainerActivityState ActivityState { get; }

        void MoveToGameplay();
    }

    public delegate void SelectedContainerChangedHandler(INodeContainer? newSelected);

    public interface INodeManager : ISerializableToDTO<INodeManagerDTO>
    {
        event SelectedContainerChangedHandler? SelectedContainerChanged;
        INodeContainer? AddNodeContainerAtPos(GameObject nodeContainerPrefab, Vector3 position);
        void HandleVoidWasClicked(Vector3 where);
        void ResetActivityState();
        void ApplyTransformation(IContainerStateTransformation transformation);
        void Clear();
        void MoveToGameplay();
        void DeleteActiveContainer();
    }

    public interface IInputInformation
    {
        Vector3 GetMouseWorldPos();
        bool WeReleasedThisFrame();
        bool WeClickedThisFrame();
        bool DelKeyWasClicked();
        bool EscapeKeyWasClicked();
        bool IsCtrlPressed();
        bool AreWeOverAGameObject();
        bool IsPressed();
        float ScrollValue();
    }
}

using UnityEngine;

namespace Assets.Prefabs.MapBuilder
{
    public interface INodeHandle
    {
        Vector3 GetCoordinates();
        bool DoesCollide(Vector3 mousePos);
        void SetTheNodeUp(INodeContainer nodeController, IInputInformation inputInformation);
        bool Active { get; set; }
    }

    public interface INodeManager
    {
        //TODO: consider adding nullable
        INodeHandle TryAddingNodeAtPoint(Vector2 point);
        INodeHandle GetActiveNode();
        bool TryDelete(INodeHandle node);
        bool TryActivatingNode(INodeHandle node);
    }

    //TODO: create a wrapper objects that has executes these methods only on a certain node
    public interface INodeContainer
    {
        bool TryDeletingThis(INodeHandle node);
        bool CanNodeMove(INodeHandle node);
        // bool IsThisNodeActive(INodeHandle node);
        bool TryActivatingTheNode(INodeHandle node);
        //TODO: consider disabling the node ability to move on the Draggable class level
        void NodeMoved(INodeHandle node);
    }

    public interface IInputInformation
    {
        Vector3 GetMouseWorldPos();
        bool WeClickedThisFrame();
        bool DelKeyWasClicked();
    }
}

#nullable enable

using System.Collections.Generic;
using UnityEngine;


namespace Assets.Prefabs.MapBuilder
{
    public sealed class NodesContainerActivityState
    {
        private ISet<INodeHandle>? activeNodes = null;

        private static readonly ISet<INodeHandle> emptyActiveNodes = new HashSet<INodeHandle>();

        // I know that this is mutable but I don't want to defensively copy it (IEnumerable is not enough)
        public ISet<INodeHandle> ActiveNodes => activeNodes is not null ? activeNodes : emptyActiveNodes;

        private NodesContainerActivityState(ISet<INodeHandle> activeNodes)
        {
            Debug.Assert(activeNodes is not null);
            this.activeNodes = activeNodes!;
            foreach (var node in this.activeNodes)
            {
                node.Active = true;
            }
        }

        //Exists only to initialize the inactive container
        private NodesContainerActivityState()
        {
        }

        public void SetNewState(NodesContainerActivityState newState)
        {
            if (activeNodes is not null)
            {
                foreach (var node in activeNodes)
                {
                    node.Active = false;
                }
            }
            activeNodes = newState.activeNodes;

            if (activeNodes is not null)
            {
                foreach (var node in activeNodes)
                {
                    node.Active = true;
                }
            }
        }

        public static NodesContainerActivityState ContainerInactive()
        {
            return new();
        }

        public static NodesContainerActivityState ContainerActiveButNoNodesSelected()
        {
            return new(new HashSet<INodeHandle>() { });
        }

        public static NodesContainerActivityState ContainerActiveWithOnlyOneNode(INodeHandle node)
        {
            return new(new HashSet<INodeHandle>() { node });
        }

        public bool IsContainerActive() { return activeNodes is not null; }

        public bool AreAnyNodesActive()
        {
            return IsContainerActive() && activeNodes!.Count > 0;
        }

        public void AddToActiveNodes(INodeHandle node)
        {
            if (!IsContainerActive())
            {
                SetNewState(ContainerActiveWithOnlyOneNode(node));
                return;
            }

            Debug.Assert(IsContainerActive());
            activeNodes!.Add(node);
            node.Active = true;
        }

        public bool RemoveFromActiveNodes(INodeHandle node)
        {
            if (!IsContainerActive())
            {
                return false;
            }

            if (activeNodes!.Remove(node))
            {
                node.Active = false;
                return true;
            }

            return false;
        }

        public bool IsTheNodeActive(INodeHandle node)
        {
            return IsContainerActive() && activeNodes!.Contains(node);
        }
    }
}

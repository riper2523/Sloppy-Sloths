#nullable enable
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Assets.Prefabs.MapBuilder;
using System;

namespace Assets.Tests.EditMode
{
    public class NodesContainerActivityStateTests
    {
        private readonly List<MockNode> createdNodes = new();

#pragma warning disable CS0067, CS0414
        private class MockNode : INodeHandle
        {
            public event Action? NodeChangedSelectionState;
            public event Action? NodeTriggered;
            public event Assets.Prefabs.MapBuilder.NodeDraggedHandler? NodeDragged;
            public event Action? NodeDragEnded;

            public bool Active { get; set; }
            public Vector3 Coordinates { get; set; }
            public void MoveByOffset(Vector3 offset) { }
            public void Delete() { }

            public void ResetState()
            {
                Active = false;
                NodeChangedSelectionState = null;
                NodeTriggered = null;
                NodeDragged = null;
                NodeDragEnded = null;
            }
        }

        private MockNode CreateMockNode()
        {
            var node = new MockNode();
            createdNodes.Add(node);
            return node;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var node in createdNodes)
            {
                node.ResetState();
            }
            createdNodes.Clear();
        }

        [Test]
        public void ContainerInactive_InitializesWithNoActiveNodes()
        {
            var state = NodesContainerActivityState.ContainerInactive();
            Assert.IsFalse(state.IsContainerActive());
            Assert.AreEqual(0, state.ActiveNodes.Count);
        }

        [Test]
        public void SetNewState_DeactivatesOldNodes_AndActivatesNewNodes()
        {
            var node1 = CreateMockNode();
            var node2 = CreateMockNode();

            var state1 = NodesContainerActivityState.ContainerActiveWithOnlyOneNode(node1);
            Assert.IsTrue(node1.Active);

            var state2 = NodesContainerActivityState.ContainerActiveWithOnlyOneNode(node2);

            state1.SetNewState(state2);

            Assert.IsFalse(node1.Active, "Old node should be deactivated");
            Assert.IsTrue(node2.Active, "New node should be activated");
        }

        [Test]
        public void ContainerActiveWithOnlyOneNode_SetsNodeToActive()
        {
            var node = CreateMockNode();
            var state = NodesContainerActivityState.ContainerActiveWithOnlyOneNode(node);

            Assert.IsTrue(node.Active);
            Assert.IsTrue(state.IsTheNodeActive(node));
        }

        [Test]
        public void AddToActiveNodes_AddsNodeToState()
        {
            var node1 = CreateMockNode();
            var node2 = CreateMockNode();
            var state = NodesContainerActivityState.ContainerActiveWithOnlyOneNode(node1);

            state.AddToActiveNodes(node2);

            Assert.IsTrue(node2.Active, "Node should be set to active");
            Assert.IsTrue(state.IsTheNodeActive(node2), "Node should be present in the active set");
        }
    }
}

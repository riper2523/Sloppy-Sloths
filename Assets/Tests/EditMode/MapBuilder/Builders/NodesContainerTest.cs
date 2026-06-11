#nullable enable
using Assets.Prefabs.MapBuilder;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using System;

namespace Assets.Tests.EditMode
{
    public class NodesContainerTests
    {
        private readonly List<GameObject> createdObjects = new();

#pragma warning disable CS0067, CS0414
        private class MockNode : INodeHandle
        {
            public event Action? NodeChangedSelectionState;
            public event Action? NodeTriggered;
            //TODO: use using, avoid such long name
            public event NodeDraggedHandler? NodeDragged;
            public event Action? NodeDragEnded;

            public bool Active { get; set; }
            public Vector3 Coordinates { get; set; }
            public void MoveByOffset(Vector3 offset) { }
            public void Delete() { }
        }

        private class MockInputInformation : MonoBehaviour, IInputInformation
        {
            public Vector3 GetMouseWorldPos() => Vector3.zero;
            public bool WeReleasedThisFrame() => false;
            public bool WeClickedThisFrame() => false;
            public bool DelKeyWasClicked() => false;
            public bool EscapeKeyWasClicked() => false;
            public bool IsCtrlPressed() => false;
            public bool VoidWasClicked() => false;
        }

        INodeContainer GetFreshInstance()
        {
            var parent = new GameObject("PolygonBuilderParent");
            parent.AddComponent<MockInputInformation>();
            createdObjects.Add(parent);

            var containerGo = new GameObject("PolygonBuilderContainer");
            containerGo.transform.SetParent(parent.transform);
            containerGo.AddComponent<SpriteShapeController>();
            containerGo.AddComponent<SpriteShapeRenderer>();
            containerGo.AddComponent<PolygonCollider2D>();
            createdObjects.Add(containerGo);

            var polygonBuilder = containerGo.AddComponent<PolygonBuilder>();
            return polygonBuilder;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in createdObjects)
            {
                if (go != null)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
            createdObjects.Clear();
        }

        [Test]
        public void SelectContainer_ContainerActivatesUponSelection()
        {
            // Arrange
            var instance = GetFreshInstance();

            // Act
            instance.SelectContainer();

            // Assert
            Assert.IsTrue(instance.ActivityState.IsContainerActive(), "Container should become active after SelectContainer()");
        }

        [Test]
        public void ResetActivityState_ContainerInactiveUponReset()
        {
            // Arrange
            var instance = GetFreshInstance();

            // Act
            instance.ResetActivityState();

            // Assert
            Assert.IsFalse(instance.ActivityState.IsContainerActive(), "Container should become inactive after ResetActivityState()");
        }

        // [Test]
        // public void AddToAct()
        // {
        //     // Arrange
        //     var instance = GetFreshInstance();
        //
        //     // Act
        //     instance.SelectContainer();
        //
        //     // Assert
        //     Assert.IsTrue(instance.ActivityState.IsContainerActive(), "Container should become active after SelectContainer()");
        // }
    }
}

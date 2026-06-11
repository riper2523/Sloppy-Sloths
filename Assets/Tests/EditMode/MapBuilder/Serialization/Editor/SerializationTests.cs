using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Newtonsoft.Json;
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Node;
using Assets.Prefabs.MapBuilder.Serialization;

namespace Assets.Tests.Prefabs.MapBuilder.Serialization
{
    [TestFixture]
    public class SerializationTests
    {
        private JsonSerializerSettings _settings;

        [OneTimeSetUp]
        public void Setup()
        {
            _settings = SerializationManager.GetSettings();
        }

        [Test]
        public void NodeHandle_RoundTrip_IsSuccessful()
        {
            // 1. Arrange
            Vector3 originalPos = new Vector3(1.5f, 2.5f, 3.5f);
            INodeHandleDTO originalDto = new NodeControllerDTO(originalPos);

            // 2. Act
            string json = JsonConvert.SerializeObject(originalDto, _settings);
            INodeHandleDTO deserializedDto = JsonConvert.DeserializeObject<INodeHandleDTO>(json, _settings);

            // 3. Assert
            Assert.IsInstanceOf<NodeControllerDTO>(deserializedDto);
            NodeControllerDTO concrete = (NodeControllerDTO)deserializedDto;
            Assert.AreEqual(originalPos, concrete.AsVector3());
            Assert.AreEqual(NodeHandleType.CIRCULAR, deserializedDto.Type);
        }

        [Test]
        public void NodeContainer_RoundTrip_IsSuccessful()
        {
            // 1. Arrange
            var nodes = new List<INodeHandleDTO>
            {
                new NodeControllerDTO(new Vector3(0, 0, 0)),
                new NodeControllerDTO(new Vector3(1, 1, 0))
            };
            INodeContainerDTO originalDto = new PolygonBuilderDTO(Vector3.zero, nodes);

            // 2. Act
            string json = JsonConvert.SerializeObject(originalDto, _settings);
            INodeContainerDTO deserializedDto = JsonConvert.DeserializeObject<INodeContainerDTO>(json, _settings);

            // 3. Assert
            Assert.IsInstanceOf<PolygonBuilderDTO>(deserializedDto);
            PolygonBuilderDTO concrete = (PolygonBuilderDTO)deserializedDto;
            Assert.AreEqual(2, concrete.NodeHandleDTOs.Count);
            Assert.AreEqual(NodeContainerType.POLYGON, deserializedDto.Type);
        }

        [Test]
        public void NodeManager_RoundTrip_IsSuccessful()
        {
            // 1. Arrange
            var nodes = new List<INodeHandleDTO> { new NodeControllerDTO(Vector3.one) };
            var containers = new List<INodeContainerDTO> { new PolygonBuilderDTO(Vector3.zero, nodes) };
            INodeManagerDTO originalDto = new OrdinaryNodeManagerDTO(containers);


            // 2. Act
            string json = JsonConvert.SerializeObject(originalDto, _settings);
            // This test is critical because it verifies the recursion fix
            INodeManagerDTO deserializedDto = JsonConvert.DeserializeObject<INodeManagerDTO>(json, _settings);

            // 3. Assert
            Assert.IsInstanceOf<OrdinaryNodeManagerDTO>(deserializedDto);
            Assert.AreEqual(1, deserializedDto.NodeContainerDTOs.Count);
            Assert.IsInstanceOf<PolygonBuilderDTO>(deserializedDto.NodeContainerDTOs[0]);
            Assert.AreEqual(NodeManagerType.ORDINARY, deserializedDto.Type);
        }

        [Test]
        public void Serialization_MaintainsPrecision()
        {
            // Arrange
            Vector3 precisePos = new Vector3(1.123456789f, 2.987654321f, 0f);
            var dto = new NodeControllerDTO(precisePos);

            // Act
            string json = JsonConvert.SerializeObject(dto, _settings);
            var deserialized = JsonConvert.DeserializeObject<INodeHandleDTO>(json, _settings);

            // Assert
            Assert.AreEqual(precisePos.x, ((NodeControllerDTO)deserialized).AsVector3().x, 0.0001f);
        }
    }
}

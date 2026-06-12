#nullable enable
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace Assets.Prefabs.MapBuilder.Serialization
{
    [CreateAssetMenu(fileName = "DTOInstantiator", menuName = "ScriptableObjects/DTOInstantiator")]
    public class DTOInstantiator : ScriptableObject
    {
        [System.Serializable]
        public struct ContainerMapping
        {
            public NodeContainerType type;
            public GameObject prefab;
        }

        [System.Serializable]
        public struct NodeMapping
        {
            public NodeHandleType type;
            public GameObject prefab;
        }

        [System.Serializable]
        public struct ManagerMapping
        {
            public NodeManagerType type;
            public GameObject prefab;
        }

        [SerializeField] private List<ContainerMapping> containerMappings = new();
        [SerializeField] private List<NodeMapping> nodeMappings = new();
        [SerializeField] private List<ManagerMapping> managerMappings = new();

        /// <summary>
        /// Instantiates a NodeManager prefab based on the DTO type and initializes it.
        /// </summary>
        public INodeManager? InstantiateNodeManager(INodeManagerDTO dto, Transform parent)
        {
            var mapping = managerMappings.Find(m => m.type == dto.Type);
            if (mapping.prefab == null)
            {
                Debug.LogError($"DTOInstantiator: No prefab mapped for NodeManagerType: {dto.Type}");
                return null;
            }

            GameObject instance = Instantiate(mapping.prefab, parent);
            var manager = instance.GetComponent<INodeManager>();

            if (manager == null)
            {
                Debug.LogError($"DTOInstantiator: Prefab for {dto.Type} is missing a component implementing INodeManager", instance);
                Destroy(instance);
                return null;
            }

            manager.SetUpUsingDTO(dto);
            return manager;
        }

        /// <summary>
        /// Instantiates a Container prefab based on the DTO type and initializes it.
        /// </summary>
        public INodeContainer? InstantiateContainer(INodeContainerDTO dto, Transform parent)
        {
            var mapping = containerMappings.Find(m => m.type == dto.Type);
            if (mapping.prefab == null)
            {
                Debug.LogError($"DTOInstantiator: No prefab mapped for ContainerType: {dto.Type}");
                return null;
            }

            // Position and Rotation will be set inside SetUpUsingDTO, so identity is fine for now
            GameObject instance = Instantiate(mapping.prefab, parent);
            var container = instance.GetComponent<INodeContainer>();

            if (container == null)
            {
                Debug.LogError($"DTOInstantiator: Prefab for {dto.Type} is missing a component implementing INodeContainer", instance);
                Destroy(instance);
                return null;
            }

            container.SetUpUsingDTO(dto);
            return container;
        }

        /// <summary>
        /// Instantiates a Node prefab based on the DTO type and initializes it.
        /// </summary>
        public INodeHandle? InstantiateNode(INodeHandleDTO dto, Transform parent)
        {
            var mapping = nodeMappings.Find(m => m.type == dto.Type);
            if (mapping.prefab == null)
            {
                Debug.LogError($"DTOInstantiator: No prefab mapped for NodeHandleType: {dto.Type}");
                return null;
            }

            GameObject instance = Instantiate(mapping.prefab, parent);
            var handle = instance.GetComponent<INodeHandle>();

            if (handle == null)
            {
                Debug.LogError($"DTOInstantiator: Prefab for {dto.Type} is missing a component implementing INodeHandle", instance);
                Destroy(instance);
                return null;
            }

            handle.SetUpUsingDTO(dto);
            return handle;
        }

        public GameObject? finishLinePrefab;

        /// <summary>
        /// Instantiates a FinishLine prefab based on the DTO and initializes it.
        /// </summary>
        public GameObject? InstantiateFinishLine(IFinishLineDTO dto, Transform parent)
        {
            if (finishLinePrefab == null)
            {
                Debug.LogError("DTOInstantiator: No prefab assigned for FinishLine");
                return null;
            }

            GameObject instance = Instantiate(finishLinePrefab, parent);
            instance.transform.position = new Vector3(dto.x, dto.y, dto.z);

            var spriteRenderer = instance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.size = new Vector2(dto.Width, dto.Height);
            }

            var collider = instance.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = new Vector2(dto.Width, dto.Height);
            }

            // FinishLineHandler is in a different assembly so we cannot check it directly here.
            // Assuming the assigned prefab is correct.

            return instance;
        }

        [SerializeField] private GameObject? starPrefab;

        /// <summary>
        /// Instantiates a Star prefab based on the DTO and initializes it.
        /// </summary>
        public GameObject? InstantiateStar(IStarDataDTO dto, Transform parent)
        {
            if (starPrefab == null)
            {
                Debug.LogError("DTOInstantiator: No prefab assigned for Star");
                return null;
            }

            GameObject instance = Instantiate(starPrefab, parent);
            instance.transform.position = new Vector3(dto.Position.x, dto.Position.y, dto.Position.z);
            
            // CollectibleStar script handles initialization on Start()
            return instance;
        }
    }
}

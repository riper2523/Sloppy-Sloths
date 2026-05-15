using UnityEngine;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager.States
{
    public interface IStateProvider
    {
        IMapBuilderManagerState GetBuilderModeState();

        IMapBuilderManagerState GetGearSelectModeState();

        IMapBuilderManagerState GetTestingModeState();
    }

    [RequireComponent(typeof(INodeManager))]
    [RequireComponent(typeof(BuilderModeState))]
    public class StateProvider : MonoBehaviour, IStateProvider
    {
        private IMapBuilderManagerState BuilderModeState;

        void Awake()
        {
            BuilderModeState = GetComponent<BuilderModeState>();
        }

        public IMapBuilderManagerState GetBuilderModeState()
        {
            Debug.Assert(BuilderModeState is not null);
            return BuilderModeState;
        }

        public IMapBuilderManagerState GetGearSelectModeState()
        {
            throw new System.NotImplementedException();
        }

        public IMapBuilderManagerState GetTestingModeState()
        {
            throw new System.NotImplementedException();
        }
    }
}

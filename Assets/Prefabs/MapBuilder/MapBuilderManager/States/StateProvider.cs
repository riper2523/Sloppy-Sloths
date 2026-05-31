using UnityEngine;
using Assets.Prefabs.MapBuilder.MapBuilderManager;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager.States
{
    public interface IStateProvider
    {
        IMapBuilderManagerState GetBuilderModeState();

        IMapBuilderManagerState GetGearSelectModeState();

        IMapBuilderManagerState GetTestingModeState();

        IMapBuilderManagerState GetStarConfigModeState();
    }

    [RequireComponent(typeof(BuilderModeState))]
    [RequireComponent(typeof(StarConfigModeState))]
    [RequireComponent(typeof(TestingModeState))]
    [RequireComponent(typeof(GearSelectionState))]
    public class StateProvider : MonoBehaviour, IStateProvider
    {
        private IMapBuilderManagerState BuilderModeState;
        private IMapBuilderManagerState GearSelectionState;
        private IMapBuilderManagerState TestingState;
        private IMapBuilderManagerState StarConfigModeState;

        void Awake()
        {
            BuilderModeState = GetComponent<BuilderModeState>();
            GearSelectionState = GetComponent<GearSelectionState>();
            TestingState = GetComponent<TestingModeState>();
            StarConfigModeState = GetComponent<StarConfigModeState>();

            Debug.Assert(BuilderModeState is not null);
            Debug.Assert(GearSelectionState is not null);
            Debug.Assert(StarConfigModeState is not null);
        }

        public IMapBuilderManagerState GetBuilderModeState()
        {
            Debug.Assert(BuilderModeState is not null);
            return BuilderModeState;
        }

        public IMapBuilderManagerState GetGearSelectModeState()
        {
            Debug.Assert(GearSelectionState is not null);
            return GearSelectionState;
        }

        public IMapBuilderManagerState GetTestingModeState()
        {
            return TestingState;
        }

        public IMapBuilderManagerState GetStarConfigModeState()
        {
            Debug.Assert(StarConfigModeState is not null);
            return StarConfigModeState;
        }
    }
}

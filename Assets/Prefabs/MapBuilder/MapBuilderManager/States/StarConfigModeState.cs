using UnityEngine;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager.States
{
    public class StarConfigModeState : MonoBehaviour, IMapBuilderManagerState
    {
        public StateID StateType => StateID.STAR_CONFIG_MODE;

        [SerializeField]
        private GameObject starConfigModeMenu = null!;

        void Awake()
        {
            Debug.Assert(starConfigModeMenu != null, "StarConfigModeState: starConfigModeMenu is not assigned!");
        }

        public void OnActivateState()
        {
            if (starConfigModeMenu != null)
                starConfigModeMenu.SetActive(true);
        }

        public void OnDeactivateState()
        {
            if (starConfigModeMenu != null)
                starConfigModeMenu.SetActive(false);
        }
    }
}

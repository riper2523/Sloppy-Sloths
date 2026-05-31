#nullable enable
using UnityEngine;

namespace Assets.Prefabs.MapBuilder.StarConfig
{
    [CreateAssetMenu(fileName = "StarConfigManager", menuName = "Scriptable Objects/StarConfigManager")]
    public class StarConfigManager : ScriptableObject
    {
        [SerializeField] private bool starForCompletion = true;
        [SerializeField] private float timeForStar = 60f;

        public bool StarForCompletion
        {
            get => starForCompletion;
            set => starForCompletion = value;
        }

        public float TimeForStar
        {
            get => timeForStar;
            set => timeForStar = value;
        }

        public void ResetToDefaults()
        {
            starForCompletion = true;
            timeForStar = 60f;
        }
    }
}

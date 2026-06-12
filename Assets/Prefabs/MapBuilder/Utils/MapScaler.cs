using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Utils
{
    public class MapScaler : MonoBehaviour
    {
        [SerializeField]
        public float ZoomSpeed = 100f;

        [SerializeField]
        public float MinZoom = 2f;

        [SerializeField]
        public float MaxZoom = 20f;

        public void ProcessScaling(Camera cam, float scrollDelta)
        {
            if (cam == null) return;

            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                float newSize = cam.orthographicSize - (scrollDelta * ZoomSpeed * 0.01f);
                cam.orthographicSize = Mathf.Clamp(newSize, MinZoom, MaxZoom);
            }
        }
    }
}

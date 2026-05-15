using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Utils
{
    public class MapMover : MonoBehaviour
    {
        private Vector3 _dragStartPos;
        private bool _isPanning = false;

        public void ProcessPanning(Camera cam, bool isPressed, bool wasPressedThisFrame, bool wasReleasedThisFrame, Vector2 mouseScreenPosition)
        {
            if (cam == null) return;

            if (wasPressedThisFrame)
            {
                float dist = Mathf.Abs(cam.transform.position.z);
                _dragStartPos = cam.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, dist));
                _isPanning = true;
            }

            if (isPressed && _isPanning)
            {
                float dist = Mathf.Abs(cam.transform.position.z);
                Vector3 currentPos = cam.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, dist));
                Vector3 difference = _dragStartPos - currentPos;

                cam.transform.position += difference;
            }

            if (wasReleasedThisFrame)
            {
                _isPanning = false;
            }
        }
    }
}

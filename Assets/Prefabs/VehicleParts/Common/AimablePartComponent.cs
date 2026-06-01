using UnityEngine;

public class AimablePartComponent : MonoBehaviour
{
    [SerializeField] private Transform barrelTransform;

    public void SetAimRotation(float exactAngle)
    {
        if (barrelTransform != null)
        {
            barrelTransform.localRotation = Quaternion.Euler(0, 0, exactAngle);
        }
    }
}
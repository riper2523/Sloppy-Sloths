using UnityEngine;

public class PropagateJointBreake : MonoBehaviour
{
    public void OnJointBreak2D(Joint2D brokenJoint)
    {
        if (transform.parent.TryGetComponent(out PartLogic partLogic))
        {
            partLogic.OnJointBreak2D(brokenJoint);
        }
        if (transform.parent.TryGetComponent(out PropagateJointBreake propagateJointBreake))
        {
            propagateJointBreake.OnJointBreak2D(brokenJoint);
        }

    }
}

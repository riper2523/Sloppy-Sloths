using UnityEngine;

public class MotorWheel : MonoBehaviour
{
    [SerializeField] private HingeJoint2D hinge;
    [SerializeField] private PartLogic partLogic;

    void Start()
    {
        JointMotor2D motor = hinge.motor;
        motor.maxMotorTorque = partLogic.actualEnginePower;
        motor.motorSpeed = -1000f;
        hinge.motor = motor;
        hinge.useMotor = false;
    }
    public void StartMotor()
    {
        hinge.useMotor = true;
    }
    public void StopMotor()
    {
        hinge.useMotor = false;
    }
}

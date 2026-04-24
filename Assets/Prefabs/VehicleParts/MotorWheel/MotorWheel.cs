using UnityEngine;

public class MotorWheel : MonoBehaviour
{
    [SerializeField] private float motorForce = 50f;
    [SerializeField] private HingeJoint2D hinge;

    void Start()
    {
        JointMotor2D motor = hinge.motor;
        motor.maxMotorTorque = motorForce;
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

using UnityEngine;

public class MotorWheel : MonoBehaviour
{
    [SerializeField] private float motorForce = 10f;
    [SerializeField] private HingeJoint2D hinge;

    void Start()
    {
        JointMotor2D motor = hinge.motor;
        motor.maxMotorTorque = motorForce;
        motor.motorSpeed = -1000f;
        hinge.motor = motor;
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

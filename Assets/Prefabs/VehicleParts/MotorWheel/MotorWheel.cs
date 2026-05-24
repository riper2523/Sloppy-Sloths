using UnityEngine;

public class MotorWheel : MonoBehaviour
{
    [SerializeField] private HingeJoint2D hinge;
    [SerializeField] private PartLogic partLogic;
    [SerializeField] private AudioSource motorSound;

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
        if (!motorSound.isPlaying)
        {
            motorSound.Play();
        }
    }
    public void StopMotor()
    {
        hinge.useMotor = false;
        if (motorSound.isPlaying)
        {
            motorSound.Stop();
        }
    }
}

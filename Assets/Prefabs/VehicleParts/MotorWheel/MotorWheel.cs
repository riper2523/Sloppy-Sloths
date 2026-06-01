using UnityEngine;

public class MotorWheel : MonoBehaviour, IPartModifier
{
    [SerializeField] private HingeJoint2D hinge;
    [SerializeField] private PartLogic partLogic;
    [SerializeField] private AudioSource motorSound;
    public float motorSpeed;
    private float motorSpeedBase = -1000f;
    private bool useMotor = false;
    void Start()
    {
        JointMotor2D motor = hinge.motor;
        motor.maxMotorTorque = partLogic.actualEnginePower;
        motor.motorSpeed = motorSpeed;
        hinge.motor = motor;
        hinge.useMotor = useMotor;
    }
    public void StartMotor()
    {
        useMotor = true;
        hinge.useMotor = true;
        if (!motorSound.isPlaying)
        {
            motorSound.Play();
        }
    }
    public void StopMotor()
    {
        useMotor = false;
        hinge.useMotor = false;
        if (motorSound.isPlaying)
        {
            motorSound.Stop();
        }
    }

    public void ResetModifier(PartLogic coreLogic)
    {
        motorSpeed = motorSpeedBase;
    }

    public void ActivateEffects(PartLogic coreLogic)
    {

    }

    public void ApplyModifiers(PartLogic coreLogic)
    {
        JointMotor2D motor = hinge.motor;
        motor.maxMotorTorque = partLogic.actualEnginePower;
        motor.motorSpeed = motorSpeed;
        hinge.motor = motor;
        hinge.useMotor = useMotor;
    }
}

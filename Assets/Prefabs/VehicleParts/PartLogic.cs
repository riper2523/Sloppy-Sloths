using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PartLogic : MonoBehaviour
{
    public UnityEvent<Vector3Int, Direction> jointBreakEvent;
    [System.NonSerialized]
    public Vector3Int gridPosition = Vector3Int.zero;
    public List<ActionReciver> actionReceivers;

    public Dictionary<Direction, PartLogic> connectedParts = new Dictionary<Direction, PartLogic>();
    private Dictionary<Joint2D, Direction> jointDirections = new Dictionary<Joint2D, Direction>();
    [SerializeField]
    private float jointStrength = 200f;
    [SerializeField]
    private float enginePower = 0f;
    [System.NonSerialized]
    public float actualJointStrength = 200f;
    [System.NonSerialized]
    public float actualEnginePower = 0f;
    private IPartModifier[] modifiers;
    private void Awake()
    {
        modifiers = GetComponentsInChildren<IPartModifier>();
        ResetPart();
    }
    public void ResetPart()
    {
        actualJointStrength = jointStrength;
        actualEnginePower = enginePower;
        gameObject.tag = "Untagged";
        foreach (var mod in modifiers)
        {
            mod.ResetModifier(this);
        }
    }
    public void ActivateEffects()
    {
        foreach (var mod in modifiers)
        {
            mod.ActivateEffects(this);
        }
    }
    public void ApplyModifiers()
    {
        foreach (var mod in modifiers)
        {
            mod.ApplyModifiers(this);
        }
    }
    public void AddConnection(Direction dir, PartLogic otherPart, Joint2D joint = null)
    {
        connectedParts[dir] = otherPart;

        if (joint != null)
        {
            jointDirections[joint] = dir;
        }
    }
    public void OnJointBreak2D(Joint2D joint)
    {

        if (jointDirections.TryGetValue(joint, out Direction brokenDir))
        {
            if (connectedParts.TryGetValue(brokenDir, out PartLogic otherPart))
            {
                Direction oppositeDir = brokenDir.Opposite();
                otherPart.HandleRemoteBreak(oppositeDir);
            }

            connectedParts.Remove(brokenDir);
            jointDirections.Remove(joint);

            jointBreakEvent.Invoke(gridPosition, brokenDir);
        }
    }
    public void HandleRemoteBreak(Direction brokenDir)
    {
        connectedParts.Remove(brokenDir);
    }
}

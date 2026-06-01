using System.Collections.Generic;
using UnityEngine;

public class SwitchModifier : MonoBehaviour, IPartModifier
{
    private PartLogic partLogic;
    private float switchState = 1f;
    void Start()
    {
        partLogic = GetComponent<PartLogic>();
    }
    public void ChangeState()
    {
        ActivateEffects(partLogic);
        switchState = -switchState;
        ActivateEffects(partLogic);
    }
    public void ActivateEffects(PartLogic coreLogic)
    {
        if (switchState == 1f)
        {
            return;
        }
        var visited = new HashSet<PartLogic>();
        var queue = new Queue<PartLogic>();

        visited.Add(coreLogic);
        queue.Enqueue(coreLogic);

        while (queue.Count > 0)
        {
            PartLogic current = queue.Dequeue();

            if (current.TryGetComponent<MotorWheel>(out var modifier))
            {
                modifier.motorSpeed *= switchState;
                modifier.ApplyModifiers(current);
            }

            foreach (var neighbor in current.connectedParts.Values)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    public void ApplyModifiers(PartLogic coreLogic)
    {


    }

    public void ResetModifier(PartLogic coreLogic)
    {
    }
}

using System.Collections.Generic;
using UnityEngine;

public class EngineModifier : MonoBehaviour, IPartModifier
{
    private float enignePower = 60f;
    private bool isActive = false;
    private PartLogic partLogic;
    public void Start()
    {
        partLogic = GetComponent<PartLogic>();
    }
    public void SetActive(bool active)
    {
        enignePower *= -1f;
        ActivateEffects(partLogic);
        enignePower *= -1f;

        isActive = active;

        ActivateEffects(partLogic);
    }

    public void ActivateEffects(PartLogic coreLogic)
    {
        if (!isActive)
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
            if (current.actualEnginePower < 0)
            {
                current.actualEnginePower -= enignePower;
            }
            else
            {
                current.actualEnginePower += enignePower;
            }
            if (current.TryGetComponent<MotorWheel>(out var motorWheel))
            {
                motorWheel.ApplyModifiers(current);
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

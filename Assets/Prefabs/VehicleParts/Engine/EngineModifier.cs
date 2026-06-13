using System.Collections.Generic;
using UnityEngine;

public class EngineModifier : MonoBehaviour, IPartModifier
{
    private float enignePower = 30f;
    
    public void ActivateEffects(PartLogic coreLogic)
    {
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

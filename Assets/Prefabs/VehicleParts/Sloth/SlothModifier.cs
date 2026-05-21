using System.Collections.Generic;
using UnityEngine;

public class SlothModifier : MonoBehaviour, IPartModifier
{
    public void ActivateEffects(PartLogic coreLogic)
    {
    }

    public void ApplyModifiers(PartLogic coreLogic)
    {

        var visited = new HashSet<PartLogic>();
        var queue = new Queue<PartLogic>();

        visited.Add(coreLogic);
        queue.Enqueue(coreLogic);

        while (queue.Count > 0)
        {
            PartLogic current = queue.Dequeue();
            if (current == null)
                continue;

            current.gameObject.tag = "Sloth";

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

    public void ResetModifier(PartLogic coreLogic)
    {
    }
}

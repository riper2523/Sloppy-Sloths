using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickablePart : MonoBehaviour, IPointerClickHandler
{

    private Dictionary<ActionReciver, bool> clickedActions = new Dictionary<ActionReciver, bool>();
    void Awake()
    {
        List<ActionReciver> actionReceivers = GetComponent<PartLogic>().actionReceivers;
        foreach (ActionReciver actionReciver in actionReceivers)
        {
            clickedActions.Add(actionReciver, false);
            actionReciver.startAction.AddListener(() => clickedActions[actionReciver] = true);
            actionReciver.stopAction.AddListener(() => clickedActions[actionReciver] = false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        foreach (ActionReciver key in clickedActions.Keys.ToList())
        {
            if (!clickedActions[key])
            {
                key.startAction.Invoke();
            }
            else
            {
                key.stopAction.Invoke();
            }
        }
    }

}

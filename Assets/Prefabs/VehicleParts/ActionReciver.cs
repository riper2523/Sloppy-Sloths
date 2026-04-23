using UnityEngine.Events;

[System.Serializable]
public struct ActionReciver
{
    public ActionType actionType;
    public UnityEvent startAction;
    public UnityEvent stopAction;
}
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildButton : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO playStartedEvent;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => playStartedEvent.RaiseEvent());
    }
}

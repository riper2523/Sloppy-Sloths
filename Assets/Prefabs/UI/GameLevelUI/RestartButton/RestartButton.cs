using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RestartButton : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO restartLevelEvent;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => restartLevelEvent.RaiseEvent());
    }
}

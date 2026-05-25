using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResumeButton : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO unpauseLevelEvent;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => unpauseLevelEvent.RaiseEvent());
    }
}

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackToMenuButton : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO backToMenuEvent;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => backToMenuEvent.RaiseEvent());
    }
}

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SidebarMenuButton : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO pauseLevelEvent;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => pauseLevelEvent.RaiseEvent());
    }
}

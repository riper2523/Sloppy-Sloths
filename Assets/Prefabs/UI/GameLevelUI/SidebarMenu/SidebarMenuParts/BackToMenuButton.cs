using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackToMenuButton : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO backToMenuEvent;
    private void Awake()
    {
        if (gameObject.name == "BackToMenuButton" && Assets.Prefabs.MapBuilder.MapBuilderManager.MapBuilderTestPreserver.IsTesting)
        {
            gameObject.SetActive(false);
            return;
        }

        GetComponent<Button>().onClick.AddListener(() => backToMenuEvent.RaiseEvent());
    }
}

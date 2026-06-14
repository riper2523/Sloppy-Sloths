using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class ReturnToBuilderButton : MonoBehaviour
{
    private void Awake()
    {
        // Only show this button if we are in testing mode
        if (!Assets.Prefabs.MapBuilder.MapBuilderManager.MapBuilderTestPreserver.IsTesting)
        {
            gameObject.SetActive(false);
            return;
        }

        GetComponent<Button>().onClick.AddListener(() =>
        {
            // Unpause the game time if it was paused by the sidebar menu
            Time.timeScale = 1f;
            SceneManager.LoadScene("MapBuilder");
        });
    }
}

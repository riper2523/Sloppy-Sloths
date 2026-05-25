using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class ReturnToMenuButton : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MenuScene";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(menuSceneName));
    }
}

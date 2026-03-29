using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private readonly string gameplaySceneName = "SampleScene";

    [SerializeField]
    private GameObject welcomeScreenPanel;

    [SerializeField]
    private GameObject browseMapsScreenPanel;

    private void Awake()
    {
        if (welcomeScreenPanel == null || browseMapsScreenPanel == null)
        {
            Debug.LogError("MainMenuController is missing panel references.");
            return;
        }

        ShowWelcomeScreen();
    }

    public void Play()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void Maps()
    {
        OpenBrowseMapsScreen();
    }

    public void OpenBrowseMapsScreen()
    {
        SetPanelVisibility(showWelcome: false, showBrowse: true);
    }

    public void OpenWelcomeScreen()
    {
        SetPanelVisibility(showWelcome: true, showBrowse: false);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowWelcomeScreen()
    {
        SetPanelVisibility(showWelcome: true, showBrowse: false);
    }

    private void SetPanelVisibility(bool showWelcome, bool showBrowse)
    {
        welcomeScreenPanel.SetActive(showWelcome);
        browseMapsScreenPanel.SetActive(showBrowse);
    }
}

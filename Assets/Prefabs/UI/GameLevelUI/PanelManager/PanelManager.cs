using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject startPanel;

    [SerializeField] private string menuSceneName = "MenuScene";

    [Header("Listening To")]
    [SerializeField] private VoidEventChannelSO playLevelEvent;
    [SerializeField] private LevelResultEventChannelSO levelCompletedEvent;
    [SerializeField] private VoidEventChannelSO restartLevelEvent;
    [SerializeField] private VoidEventChannelSO exitLevelEvent;

    private GameObject activePanel;

    private void Awake()
    {
        gamePanel.SetActive(false);
        buildPanel.SetActive(false);
        winPanel.SetActive(false);

        startPanel.SetActive(true);
        activePanel = startPanel;
    }

    private void OnEnable()
    {
        playLevelEvent.OnEventRaised += ShowGamePanel;
        levelCompletedEvent.OnEventRaised += ShowWinPanel;
        restartLevelEvent.OnEventRaised += ShowBuildPanel;
        exitLevelEvent.OnEventRaised += BackToMenu;
    }

    private void OnDisable()
    {
        playLevelEvent.OnEventRaised -= ShowGamePanel;
        levelCompletedEvent.OnEventRaised -= ShowWinPanel;
        restartLevelEvent.OnEventRaised -= ShowBuildPanel;
        exitLevelEvent.OnEventRaised -= BackToMenu;
    }

    public void ShowGamePanel() => SwitchPanel(gamePanel);
    public void ShowBuildPanel() => SwitchPanel(buildPanel);
    public void ShowWinPanel(LevelResult result)
    {
        SwitchPanel(winPanel);
        winPanel.GetComponent<WinPanelUI>().DisplayResults(result);
    }

    private void SwitchPanel(GameObject newPanel)
    {
        if (activePanel != null)
        {
            activePanel.SetActive(false);
        }
        
        newPanel.SetActive(true);
        activePanel = newPanel;
    }

    public void BackToMenu() => SceneManager.LoadScene(menuSceneName);
}

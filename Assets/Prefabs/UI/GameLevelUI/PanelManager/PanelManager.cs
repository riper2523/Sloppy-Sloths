using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject sidebarMenu;
    [SerializeField] private GameObject sidebarMenuButton;

    [SerializeField] private string menuSceneName = "MenuScene";

    [Header("Listening To")]
    [SerializeField] private VoidEventChannelSO playLevelEvent;
    [SerializeField] private LevelResultEventChannelSO levelCompletedEvent;
    [SerializeField] private VoidEventChannelSO restartLevelEvent;
    [SerializeField] private VoidEventChannelSO pauseLevelEvent;
    [SerializeField] private VoidEventChannelSO unpauseLevelEvent;
    [SerializeField] private VoidEventChannelSO exitLevelEvent;

    private GameObject activePanel;

    private void Awake()
    {
        gamePanel.SetActive(false);
        buildPanel.SetActive(false);
        winPanel.SetActive(false);
        sidebarMenu.SetActive(false);

        startPanel.SetActive(true);
        sidebarMenuButton.SetActive(true);
        activePanel = startPanel;
    }

    private void OnEnable()
    {
        playLevelEvent.OnEventRaised += ShowGamePanel;
        levelCompletedEvent.OnEventRaised += ShowWinPanel;
        restartLevelEvent.OnEventRaised += ShowBuildPanel;

        pauseLevelEvent.OnEventRaised += ShowSidebarMenu;
        unpauseLevelEvent.OnEventRaised += HideSidebarMenu;

        exitLevelEvent.OnEventRaised += BackToMenu;
    }

    private void OnDisable()
    {
        playLevelEvent.OnEventRaised -= ShowGamePanel;
        levelCompletedEvent.OnEventRaised -= ShowWinPanel;
        restartLevelEvent.OnEventRaised -= ShowBuildPanel;

        pauseLevelEvent.OnEventRaised -= ShowSidebarMenu;
        unpauseLevelEvent.OnEventRaised -= HideSidebarMenu;

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

    public void ShowSidebarMenu()
    {
        Debug.Log("sidebar menu on");
        sidebarMenuButton.SetActive(false);
        sidebarMenu.SetActive(true);
        //TODO: pause the game
    }

    public void HideSidebarMenu()
    {
        Debug.Log("sidebar menu off");
        sidebarMenuButton.SetActive(true);
        sidebarMenu.SetActive(false);
        //TODO: unpause the game
    }

    public void BackToMenu() => SceneManager.LoadScene(menuSceneName);
}

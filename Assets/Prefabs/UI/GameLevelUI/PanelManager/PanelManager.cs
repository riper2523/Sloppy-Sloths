using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject startPanel;

    [Header("Listening To")]
    [SerializeField] private LevelDataEventChannelSO loadLevelEvent;
    [SerializeField] private VoidEventChannelSO playLevelEvent;
    [SerializeField] private VoidEventChannelSO levelCompletedEvent;
    [SerializeField] private VoidEventChannelSO restartLevelEvent;

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
        loadLevelEvent.OnEventRaised += onLoadLevel;
        playLevelEvent.OnEventRaised += ShowGamePanel;
        levelCompletedEvent.OnEventRaised += ShowWinPanel;
        restartLevelEvent.OnEventRaised += ShowBuildPanel;
    }

    private void OnDisable()
    {
        loadLevelEvent.OnEventRaised -= onLoadLevel;
        playLevelEvent.OnEventRaised -= ShowGamePanel;
        levelCompletedEvent.OnEventRaised -= ShowWinPanel;
        restartLevelEvent.OnEventRaised -= ShowBuildPanel;
    }

    public void ShowGamePanel() => SwitchPanel(gamePanel);
    public void ShowBuildPanel() => SwitchPanel(buildPanel);
    public void ShowWinPanel()
    {
        SwitchPanel(winPanel);
        winPanel.GetComponent<WinPanelUI>().DisplayResults();
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

    private void onLoadLevel(LevelData data) => ShowBuildPanel();
}

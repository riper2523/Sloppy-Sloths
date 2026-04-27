using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject startPanel;

    private GameObject activePanel;

    private void Awake()
    {
        gamePanel.SetActive(false);
        buildPanel.SetActive(false);
        winPanel.SetActive(false);

        startPanel.SetActive(true);
        activePanel = startPanel;
    }

    public void ShowGamePanel() => SwitchPanel(gamePanel);
    public void ShowBuildPanel() => SwitchPanel(buildPanel);
    public void ShowWinPanel(bool[] earnedStars)
    {
        SwitchPanel(winPanel);
        winPanel.GetComponent<WinPanelUI>().DisplayResults(earnedStars);
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
}

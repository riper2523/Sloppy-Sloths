using Unity.VisualScripting;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField]
    private GameObject gamePanel;
    [SerializeField]
    private GameObject buildPanel;
    [SerializeField]
    private GameObject winPanel;

    [SerializeField]
    private GameObject startPanel;

    private GameObject activePanel;

    void Awake()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        buildPanel.SetActive(false);
        winPanel.SetActive(false);

        startPanel.SetActive(true);
        activePanel = startPanel;
    }

    public void ShowGamePanel()
    {
        if (activePanel != null)
            activePanel.SetActive(false);
        gamePanel.SetActive(true);
        activePanel = gamePanel;
    }

    public void ShowBuildPanel()
    {
        if (activePanel != null)
            activePanel.SetActive(false);
        buildPanel.SetActive(true);
        activePanel = buildPanel;
    }

    public void ShowWinPanel()
    {
        if (activePanel != null)
            activePanel.SetActive(false);
        winPanel.SetActive(true);
        activePanel = winPanel;
    }
}

using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject campaignPanel;
    [SerializeField] private GameObject levelSelectionPanel;

    private void Start()
    {
        ShowMainPanel();
    }

    private void HideAllPanels()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (campaignPanel) campaignPanel.SetActive(false);
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);
    }

    public void ShowMainPanel()
    {
        HideAllPanels();
        if (mainPanel) mainPanel.SetActive(true);
    }

    public void ShowSettingsPanel()
    {
        HideAllPanels();
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void ShowCampaignPanel()
    {
        HideAllPanels();
        if (campaignPanel) campaignPanel.SetActive(true);
    }

    public void ShowLevelSelectionPanel()
    {
        HideAllPanels();
        if (levelSelectionPanel) levelSelectionPanel.SetActive(true);
    }

    public void QuitGame()
    {
        //build version
        Application.Quit();

        //in editor
        UnityEditor.EditorApplication.isPlaying = false;

        Debug.Log("The game should have shut down");
    }
}

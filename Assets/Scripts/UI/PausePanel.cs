using UnityEngine;
using UnityEngine.UI;

public class PausePanel : UIPanel
{
    [Header("Pause Menu")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    protected override void OnShowComplete()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(OpenInventory);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    protected override void OnHideComplete()
    {
        RemoveAllListeners();
    }

    private void RemoveAllListeners()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
        if (inventoryButton != null) inventoryButton.onClick.RemoveAllListeners();
        if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
        if (quitButton != null) quitButton.onClick.RemoveAllListeners();
    }

    private void ResumeGame()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Pause);
    }

    private void OpenInventory()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Pause);
        UIManager.Instance?.OpenPanel(UIPanelType.Inventory);
    }

    private void OpenSettings()
    {
        UIManager.Instance?.OpenPanel(UIPanelType.Settings);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
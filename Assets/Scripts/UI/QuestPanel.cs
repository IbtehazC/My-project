using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestPanel : UIPanel
{
    [Header("Quest Elements")]
    [SerializeField] private TextMeshProUGUI questInfoText;
    [SerializeField] private Button closeButton;

    protected override void OnShowComplete()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (questInfoText != null)
        {
            questInfoText.text = "Quest System\n\nComing Soon!\n\n• Quest tracking\n• Objectives\n• Rewards";
        }
    }

    protected override void OnHideComplete()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();
    }

    private void ClosePanel()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Quest);
    }
}
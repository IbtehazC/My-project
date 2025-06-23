using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : UIPanel
{
    [Header("Dialogue Elements")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;

    protected override void OnShowComplete()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueDialogue);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseDialogue);
    }

    protected override void OnHideComplete()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveAllListeners();

        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();
    }

    public void ShowMessage(string message)
    {
        if (dialogueText != null)
        {
            dialogueText.text = message;
        }
    }

    private void ContinueDialogue()
    {
        // Placeholder - implement when dialogue system is ready
        CloseDialogue();
    }

    private void CloseDialogue()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Dialogue);
    }
}
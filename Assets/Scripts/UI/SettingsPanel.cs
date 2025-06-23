using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : UIPanel
{
    [Header("Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private Button closeButton;

    protected override void OnShowComplete()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioListener.volume;
            masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (muteToggle != null)
        {
            muteToggle.isOn = AudioListener.pause;
            muteToggle.onValueChanged.AddListener(OnMuteToggled);
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettings);
    }

    protected override void OnHideComplete()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveAllListeners();

        if (muteToggle != null)
            muteToggle.onValueChanged.RemoveAllListeners();

        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();
    }

    private void OnVolumeChanged(float volume)
    {
        AudioListener.volume = volume;
    }

    private void OnMuteToggled(bool isMuted)
    {
        AudioListener.pause = isMuted;
    }

    private void CloseSettings()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Settings);
    }
}
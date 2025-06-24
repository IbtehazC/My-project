using UnityEngine;
using System.Collections.Generic;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Input Settings")]
    [SerializeField] private float inputCooldown = 0.2f; // Prevent spam clicking

    // Current active panels
    private List<UIPanel> activePanels = new List<UIPanel>();
    private Dictionary<UIPanelType, UIPanel> registeredPanels = new Dictionary<UIPanelType, UIPanel>();

    // Input debouncing
    private float lastInputTime = 0f;

    // UI State
    public bool IsAnyPanelOpen => activePanels.Count > 0;
    public bool IsGameplayUI => activePanels.Count == 0 || (activePanels.Count == 1 && activePanels[0].PanelType == UIPanelType.HUD);

    // Events
    public Action<UIPanelType> OnPanelOpened;
    public Action<UIPanelType> OnPanelClosed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeUIManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeUIManager()
    {
        // Register all panels
        RegisterPanel(UIPanelType.HUD, hudPanel);
        RegisterPanel(UIPanelType.Inventory, inventoryPanel);
        RegisterPanel(UIPanelType.Dialogue, dialoguePanel);
        RegisterPanel(UIPanelType.Quest, questPanel);
        RegisterPanel(UIPanelType.Pause, pausePanel);
        RegisterPanel(UIPanelType.Settings, settingsPanel);

        // Initialize with HUD only
        CloseAllPanels();
        OpenPanel(UIPanelType.HUD);
    }

    private void RegisterPanel(UIPanelType panelType, GameObject panelGameObject)
    {
        if (panelGameObject != null)
        {
            UIPanel panel = panelGameObject.GetComponent<UIPanel>();
            if (panel == null)
            {
                panel = panelGameObject.AddComponent<UIPanel>();
            }

            panel.Initialize(panelType);
            registeredPanels[panelType] = panel;
        }
    }

    private void Update()
    {
        HandleUIInput();
    }

    private void HandleUIInput()
    {
        // Input cooldown to prevent spam
        if (Time.unscaledTime - lastInputTime < inputCooldown) return;

        bool inputReceived = false;

        // Inventory toggle
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
        {
            TogglePanel(UIPanelType.Inventory);
            inputReceived = true;
        }

        // Quest log toggle
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TogglePanel(UIPanelType.Quest);
            inputReceived = true;
        }

        // Pause menu toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPanelOpen(UIPanelType.Pause))
            {
                ClosePanel(UIPanelType.Pause);
            }
            else
            {
                CloseAllPanelsExcept(UIPanelType.HUD);
                OpenPanel(UIPanelType.Pause);
            }
            inputReceived = true;
        }

        // Update last input time if any input was received
        if (inputReceived)
        {
            lastInputTime = Time.unscaledTime;
        }
    }

    public void OpenPanel(UIPanelType panelType)
    {
        if (!registeredPanels.ContainsKey(panelType)) return;

        UIPanel panel = registeredPanels[panelType];

        // Don't open if already transitioning or already open
        if (panel.IsTransitioning || activePanels.Contains(panel)) return;

        if (!activePanels.Contains(panel))
        {
            activePanels.Add(panel);
            panel.Show();

            OnPanelOpened?.Invoke(panelType);
            HandlePanelSpecialLogic(panelType, true);
        }
    }

    public void ClosePanel(UIPanelType panelType)
    {
        if (!registeredPanels.ContainsKey(panelType)) return;

        UIPanel panel = registeredPanels[panelType];

        // Don't close if already transitioning or already closed
        if (panel.IsTransitioning || !activePanels.Contains(panel)) return;

        if (activePanels.Contains(panel))
        {
            activePanels.Remove(panel);
            panel.Hide();

            OnPanelClosed?.Invoke(panelType);
            HandlePanelSpecialLogic(panelType, false);
        }
    }

    public void TogglePanel(UIPanelType panelType)
    {
        if (IsPanelOpen(panelType))
        {
            ClosePanel(panelType);
        }
        else
        {
            OpenPanel(panelType);
        }
    }

    public bool IsPanelOpen(UIPanelType panelType)
    {
        if (!registeredPanels.ContainsKey(panelType)) return false;
        return activePanels.Contains(registeredPanels[panelType]);
    }

    public void CloseAllPanels()
    {
        List<UIPanel> panelsToClose = new List<UIPanel>(activePanels);
        foreach (UIPanel panel in panelsToClose)
        {
            ClosePanel(panel.PanelType);
        }
    }

    public void CloseAllPanelsExcept(UIPanelType exception)
    {
        List<UIPanel> panelsToClose = new List<UIPanel>();

        foreach (UIPanel panel in activePanels)
        {
            if (panel.PanelType != exception)
            {
                panelsToClose.Add(panel);
            }
        }

        foreach (UIPanel panel in panelsToClose)
        {
            ClosePanel(panel.PanelType);
        }
    }

    private void HandlePanelSpecialLogic(UIPanelType panelType, bool isOpening)
    {
        switch (panelType)
        {
            case UIPanelType.Pause:
                Time.timeScale = isOpening ? 0f : 1f;
                break;

            case UIPanelType.Inventory:
            case UIPanelType.Quest:
                SetPlayerControlsEnabled(!isOpening);
                break;
        }
    }

    private void SetPlayerControlsEnabled(bool enabled)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetState(enabled ? PlayerState.Normal : PlayerState.Interacting);
        }
    }

    public T GetPanel<T>() where T : UIPanel
    {
        foreach (UIPanel panel in registeredPanels.Values)
        {
            if (panel is T)
            {
                return (T)panel;
            }
        }
        return null;
    }

    public UIPanel GetPanel(UIPanelType panelType)
    {
        registeredPanels.TryGetValue(panelType, out UIPanel panel);
        return panel;
    }
}

public enum UIPanelType
{
    HUD,
    Inventory,
    Dialogue,
    Quest,
    Pause,
    Settings
}
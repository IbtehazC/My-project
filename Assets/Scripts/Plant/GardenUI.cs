using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GardenUI : MonoBehaviour
{
    [Header("Garden HUD")]
    [SerializeField] private GameObject gardenHUD;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI weatherText;
    [SerializeField] private Button harvestAllButton;
    [SerializeField] private Button waterAllButton;
    [SerializeField] private Button gardenInfoButton;

    [Header("Quick Actions")]
    [SerializeField] private KeyCode harvestAllKey = KeyCode.H;
    [SerializeField] private KeyCode waterAllKey = KeyCode.W;
    [SerializeField] private KeyCode gardenInfoKey = KeyCode.G;

    private bool isInGardenArea = false;

    private void Start()
    {
        SetupUI();
        UpdateGardenHUD();

        // Subscribe to garden events
        if (GardenManager.Instance != null)
        {
            GardenManager.Instance.OnGardenStatsChanged += OnGardenStatsChanged;
        }

        // Subscribe to time events
        SimpleTimeManager.OnTimeChanged += OnTimeChanged;
    }

    private void Update()
    {
        HandleQuickActions();
    }

    private void SetupUI()
    {
        // Hide garden HUD initially
        if (gardenHUD != null)
        {
            gardenHUD.SetActive(false);
        }

        // Setup button events
        if (harvestAllButton != null)
        {
            harvestAllButton.onClick.AddListener(() => GardenManager.Instance?.HarvestAllReadyPlants());
        }

        if (waterAllButton != null)
        {
            waterAllButton.onClick.AddListener(() => GardenManager.Instance?.WaterAllPlants());
        }

        if (gardenInfoButton != null)
        {
            gardenInfoButton.onClick.AddListener(ToggleGardenInfo);
        }
    }

    private void HandleQuickActions()
    {
        if (!isInGardenArea) return;

        // Quick harvest all
        if (Input.GetKeyDown(harvestAllKey))
        {
            GardenManager.Instance?.HarvestAllReadyPlants();
        }

        // Quick water all
        if (Input.GetKeyDown(waterAllKey))
        {
            GardenManager.Instance?.WaterAllPlants();
        }

        // Toggle garden info
        if (Input.GetKeyDown(gardenInfoKey))
        {
            ToggleGardenInfo();
        }
    }

    private void OnTimeChanged(System.DateTime newTime)
    {
        UpdateTimeDisplay(newTime);
    }

    private void OnGardenStatsChanged(int total, int harvestable, int needWater)
    {
        UpdateActionButtons(harvestable, needWater);
    }

    private void UpdateGardenHUD()
    {
        if (SimpleTimeManager.Instance != null)
        {
            UpdateTimeDisplay(SimpleTimeManager.Instance.CurrentTime);
        }
    }

    private void UpdateTimeDisplay(System.DateTime time)
    {
        if (timeText != null)
        {
            timeText.text = $"{time:HH:mm} - Day {time.DayOfYear}";
        }

        // Optional: Add weather system integration
        if (weatherText != null)
        {
            weatherText.text = "Sunny"; // Placeholder
        }
    }

    private void UpdateActionButtons(int harvestable, int needWater)
    {
        // Enable/disable buttons based on garden state
        if (harvestAllButton != null)
        {
            harvestAllButton.interactable = harvestable > 0;

            TextMeshProUGUI buttonText = harvestAllButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Harvest All ({harvestable})";
            }
        }

        if (waterAllButton != null)
        {
            waterAllButton.interactable = needWater > 0;

            TextMeshProUGUI buttonText = waterAllButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Water All ({needWater})";
            }
        }
    }

    private void ToggleGardenInfo()
    {
        if (GardenManager.Instance != null)
        {
            GardenManager.Instance.ToggleGardenInfoPanel();
        }
    }

    // Called by garden area trigger
    public void SetInGardenArea(bool inArea)
    {
        isInGardenArea = inArea;

        if (gardenHUD != null)
        {
            gardenHUD.SetActive(inArea);
        }

        if (inArea)
        {
            UpdateGardenHUD();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GardenManager.Instance != null)
        {
            GardenManager.Instance.OnGardenStatsChanged -= OnGardenStatsChanged;
        }

        SimpleTimeManager.OnTimeChanged -= OnTimeChanged;
    }
}
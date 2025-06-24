// GardenManager.cs - Central garden system controller
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GardenManager : MonoBehaviour
{
    [Header("Garden Database")]
    [SerializeField] private PlantDatabase plantDatabase;

    [Header("Garden Plots")]
    [SerializeField] private List<GardenPlot> gardenPlots = new List<GardenPlot>();

    [Header("Plant Selection UI")]
    [SerializeField] private GameObject plantSelectionPanel;
    [SerializeField] private Transform plantButtonContainer;
    [SerializeField] private GameObject plantButtonPrefab;
    [SerializeField] private Button closePlantSelectionButton;

    [Header("Garden Info UI")]
    [SerializeField] private GameObject gardenInfoPanel;
    [SerializeField] private TextMeshProUGUI totalPlantsText;
    [SerializeField] private TextMeshProUGUI readyToHarvestText;
    [SerializeField] private TextMeshProUGUI needWateringText;

    [Header("Settings")]
    [SerializeField] private bool autoFindPlots = true;
    [SerializeField] private bool showGardenInfo = true;
    [SerializeField] private float infoUpdateInterval = 5f;

    public static GardenManager Instance { get; private set; }

    // Current state
    private GardenPlot selectedPlot;
    private List<Button> plantSelectionButtons = new List<Button>();
    private float lastInfoUpdate = 0f;

    // Events
    public System.Action<int, int, int> OnGardenStatsChanged; // total, harvestable, needWater

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeGarden();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupGardenPlots();
        SetupUI();
        UpdateGardenInfo();
    }

    private void Update()
    {
        // Periodically update garden info
        if (showGardenInfo && Time.time - lastInfoUpdate >= infoUpdateInterval)
        {
            UpdateGardenInfo();
            lastInfoUpdate = Time.time;
        }
    }

    private void InitializeGarden()
    {
        if (autoFindPlots)
        {
            FindAllGardenPlots();
        }
    }

    private void FindAllGardenPlots()
    {
        GardenPlot[] foundPlots = FindObjectsOfType<GardenPlot>();
        gardenPlots.Clear();
        gardenPlots.AddRange(foundPlots);

        Debug.Log($"Found {gardenPlots.Count} garden plots");
    }

    private void SetupGardenPlots()
    {
        foreach (GardenPlot plot in gardenPlots)
        {
            if (plot != null)
            {
                // Subscribe to plot events
                plot.OnPlantStageChanged += OnPlotStageChanged;
                plot.OnPlantHarvested += OnPlotHarvested;
            }
        }
    }

    private void SetupUI()
    {
        // Hide plant selection initially
        if (plantSelectionPanel != null)
        {
            plantSelectionPanel.SetActive(false);
        }

        // Setup close button
        if (closePlantSelectionButton != null)
        {
            closePlantSelectionButton.onClick.AddListener(HidePlantSelectionUI);
        }

        // Create plant selection buttons
        CreatePlantSelectionButtons();

        // Setup garden info panel
        if (gardenInfoPanel != null)
        {
            gardenInfoPanel.SetActive(showGardenInfo);
        }
    }

    private void CreatePlantSelectionButtons()
    {
        if (plantDatabase == null || plantButtonContainer == null || plantButtonPrefab == null)
            return;

        // Clear existing buttons
        foreach (Button button in plantSelectionButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        plantSelectionButtons.Clear();

        // Create button for each plant
        List<PlantData> availablePlants = plantDatabase.GetAllPlants();

        foreach (PlantData plantData in availablePlants)
        {
            if (plantData?.plant == null) continue;

            GameObject buttonObj = Instantiate(plantButtonPrefab, plantButtonContainer);
            Button button = buttonObj.GetComponent<Button>();

            // Setup button appearance
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = plantData.plant.displayName;
            }

            // Add plant icon if available
            Image buttonIcon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
            if (buttonIcon != null && plantData.plant.stageVisuals.Length > 0)
            {
                // Use mature stage visual as icon
                var matureVisual = plantData.plant.stageVisuals.FirstOrDefault(v => v.stage == PlantGrowthStage.Mature);
                if (matureVisual != null && matureVisual.sprite != null)
                {
                    buttonIcon.sprite = matureVisual.sprite;
                }
            }

            // Setup button callback
            string plantId = plantData.plant.plantId; // Capture for closure
            button.onClick.AddListener(() => OnPlantSelected(plantId));

            plantSelectionButtons.Add(button);
        }
    }

    public void ShowPlantSelectionUI(GardenPlot plot)
    {
        if (plantSelectionPanel == null) return;

        selectedPlot = plot;
        plantSelectionPanel.SetActive(true);

        // Pause time while selecting
        if (UIManager.Instance != null)
        {
            // This would pause player movement
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.SetState(PlayerState.Interacting);
            }
        }
    }

    public void HidePlantSelectionUI()
    {
        if (plantSelectionPanel != null)
        {
            plantSelectionPanel.SetActive(false);
        }

        selectedPlot = null;

        // Resume player movement
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetState(PlayerState.Normal);
        }
    }

    private void OnPlantSelected(string plantId)
    {
        if (selectedPlot == null) return;

        // Check if player has seeds (optional requirement)
        // For now, we'll allow unlimited planting

        bool success = selectedPlot.PlantSeed(plantId);

        if (success)
        {
            HidePlantSelectionUI();
            UpdateGardenInfo();
        }
        else
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(
                    "Cannot plant here!",
                    NotificationType.Warning
                );
            }
        }
    }

    private void OnPlotStageChanged(GardenPlot plot, PlantGrowthStage newStage)
    {
        UpdateGardenInfo();

        // Optional: Special handling for certain stages
        if (newStage == PlantGrowthStage.Harvestable)
        {
            // Could add special effects, sounds, etc.
        }
    }

    private void OnPlotHarvested(GardenPlot plot, string itemId, int quantity)
    {
        UpdateGardenInfo();

        // Optional: Track harvest statistics
        Debug.Log($"Harvested {quantity}x {itemId} from plot {plot.PlotId}");
    }

    private void UpdateGardenInfo()
    {
        if (!showGardenInfo) return;

        // Calculate garden statistics
        int totalPlants = 0;
        int readyToHarvest = 0;
        int needWatering = 0;

        foreach (GardenPlot plot in gardenPlots)
        {
            if (plot != null && !plot.IsEmpty)
            {
                totalPlants++;

                if (plot.HasMaturePlant)
                {
                    readyToHarvest++;
                }

                if (plot.NeedsWatering)
                {
                    needWatering++;
                }
            }
        }

        // Update UI
        if (totalPlantsText != null)
        {
            totalPlantsText.text = $"Total Plants: {totalPlants}";
        }

        if (readyToHarvestText != null)
        {
            readyToHarvestText.text = $"Ready to Harvest: {readyToHarvest}";
            readyToHarvestText.color = readyToHarvest > 0 ? Color.green : Color.white;
        }

        if (needWateringText != null)
        {
            needWateringText.text = $"Need Watering: {needWatering}";
            needWateringText.color = needWatering > 0 ? Color.yellow : Color.white;
        }

        // Fire event
        OnGardenStatsChanged?.Invoke(totalPlants, readyToHarvest, needWatering);
    }

    // Public utility methods
    public PlantData GetPlantData(string plantId)
    {
        return plantDatabase?.GetPlant(plantId);
    }

    public void ToggleGardenInfoPanel()
    {
        if (gardenInfoPanel != null)
        {
            bool isActive = gardenInfoPanel.activeInHierarchy;
            gardenInfoPanel.SetActive(!isActive);
        }
    }

    public bool IsGardenInfoVisible()
    {
        return gardenInfoPanel != null && gardenInfoPanel.activeInHierarchy;
    }

    public List<GardenPlot> GetAllPlots()
    {
        return new List<GardenPlot>(gardenPlots);
    }

    public List<GardenPlot> GetPlotsWithMaturePlants()
    {
        return gardenPlots.Where(plot => plot != null && plot.HasMaturePlant).ToList();
    }

    public List<GardenPlot> GetPlotsNeedingWater()
    {
        return gardenPlots.Where(plot => plot != null && plot.NeedsWatering).ToList();
    }

    public int GetTotalPlantCount()
    {
        return gardenPlots.Count(plot => plot != null && !plot.IsEmpty);
    }

    // Auto-harvest all ready plants (convenience method)
    [ContextMenu("Harvest All Ready Plants")]
    public void HarvestAllReadyPlants()
    {
        var readyPlots = GetPlotsWithMaturePlants();
        int totalHarvested = 0;

        foreach (GardenPlot plot in readyPlots)
        {
            HarvestResult result = plot.HarvestPlant();
            if (result != null)
            {
                totalHarvested += result.quantity;
            }
        }

        if (totalHarvested > 0 && NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification(
                $"Auto-harvested {totalHarvested} herbs!",
                NotificationType.Success
            );
        }
    }

    // Water all plants that need it (convenience method)
    [ContextMenu("Water All Plants")]
    public void WaterAllPlants()
    {
        var thirstyPlots = GetPlotsNeedingWater();
        int totalWatered = 0;

        foreach (GardenPlot plot in thirstyPlots)
        {
            if (plot.WaterPlant())
            {
                totalWatered++;
            }
        }

        if (totalWatered > 0 && NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification(
                $"Watered {totalWatered} plants!",
                NotificationType.Info
            );
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from plot events
        foreach (GardenPlot plot in gardenPlots)
        {
            if (plot != null)
            {
                plot.OnPlantStageChanged -= OnPlotStageChanged;
                plot.OnPlantHarvested -= OnPlotHarvested;
            }
        }
    }
}
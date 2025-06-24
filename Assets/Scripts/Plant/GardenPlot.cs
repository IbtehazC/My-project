// GardenPlot.cs - Individual plant plot management
using UnityEngine;
using System;

public class GardenPlot : MonoBehaviour
{
    [Header("Plot Settings")]
    [SerializeField] private int plotId = 0;
    [SerializeField] private bool startEmpty = true;

    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer plantRenderer;
    [SerializeField] private SpriteRenderer soilRenderer;
    [SerializeField] private GameObject waterDroplets; // Optional water effect

    [Header("Interaction")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionRange = 1.5f;

    // Current plant state
    private PlantInstance currentPlant;
    private bool isWatered = false;
    private DateTime lastWateredTime;

    // Events
    public Action<GardenPlot, PlantGrowthStage> OnPlantStageChanged;
    public Action<GardenPlot, string, int> OnPlantHarvested; // plot, itemId, quantity

    // Properties
    public bool IsEmpty => currentPlant == null;
    public bool HasMaturePlant => currentPlant != null && currentPlant.growthStage == PlantGrowthStage.Harvestable;
    public bool NeedsWatering => currentPlant != null && !isWatered && currentPlant.plantData.plant.requiresWatering;
    public PlantInstance CurrentPlant => currentPlant;
    public int PlotId => plotId;

    private void Start()
    {
        InitializePlot();
        UpdateVisuals();

        // Subscribe to time events
        SimpleTimeManager.OnTimeChanged += OnTimeAdvanced;
        SimpleTimeManager.OnHourChanged += OnHourPassed;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        SimpleTimeManager.OnTimeChanged -= OnTimeAdvanced;
        SimpleTimeManager.OnHourChanged -= OnHourPassed;
    }

    private void InitializePlot()
    {
        if (startEmpty)
        {
            currentPlant = null;
        }

        // Set default soil appearance
        if (soilRenderer != null)
        {
            soilRenderer.color = Color.white; // Normal soil color
        }

        // Hide water effect
        if (waterDroplets != null)
        {
            waterDroplets.SetActive(false);
        }
    }

    public bool CanPlant()
    {
        return IsEmpty;
    }

    public bool CanWater()
    {
        return !IsEmpty && !isWatered && currentPlant.plantData.plant.requiresWatering;
    }

    public bool CanHarvest()
    {
        return HasMaturePlant;
    }

    public bool PlantSeed(string plantId)
    {
        if (!CanPlant()) return false;

        PlantData plantData = GardenManager.Instance?.GetPlantData(plantId);
        if (plantData == null)
        {
            Debug.LogWarning($"Plant data not found for ID: {plantId}");
            return false;
        }

        // Create new plant instance
        currentPlant = new PlantInstance
        {
            plantData = plantData,
            plantedTime = SimpleTimeManager.Instance.CurrentTime,
            growthStage = PlantGrowthStage.Planted,
            timesHarvested = 0,
            lastGrowthUpdate = SimpleTimeManager.Instance.CurrentTime
        };

        // Reset watering state
        isWatered = false;

        UpdateVisuals();
        OnPlantStageChanged?.Invoke(this, currentPlant.growthStage);

        // Show notification
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification(
                $"Planted {plantData.plant.displayName}",
                NotificationType.Success
            );
        }

        return true;
    }

    public bool WaterPlant()
    {
        if (!CanWater()) return false;

        isWatered = true;
        lastWateredTime = SimpleTimeManager.Instance.CurrentTime;

        UpdateVisuals();

        // Show notification
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification(
                "Plant watered! Growth speed increased.",
                NotificationType.Info
            );
        }

        return true;
    }

    public HarvestResult HarvestPlant()
    {
        if (!CanHarvest()) return null;

        Plant plantDef = currentPlant.plantData.plant;

        // Calculate harvest amount (base + random bonus)
        int harvestAmount = UnityEngine.Random.Range(plantDef.baseHarvestAmount, plantDef.maxHarvestAmount + 1);

        // Create harvest result
        HarvestResult result = new HarvestResult
        {
            itemId = plantDef.harvestItemId,
            quantity = harvestAmount,
            plantName = plantDef.displayName
        };

        // Add to inventory
        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(result.itemId, result.quantity);
        }

        // Update plant state
        currentPlant.timesHarvested++;

        // Check if plant can be harvested again
        if (currentPlant.timesHarvested >= plantDef.maxHarvests)
        {
            // Plant is exhausted, remove it
            currentPlant = null;
            isWatered = false;
        }
        else
        {
            // Reset growth for next harvest
            currentPlant.growthStage = PlantGrowthStage.Growing;
            currentPlant.lastGrowthUpdate = SimpleTimeManager.Instance.CurrentTime;
            isWatered = false; // Need to water again
        }

        UpdateVisuals();
        OnPlantHarvested?.Invoke(this, result.itemId, result.quantity);

        // Show notification
        if (NotificationManager.Instance != null)
        {
            string message = result.quantity > 1 ?
                $"Harvested {result.quantity}x {result.plantName}!" :
                $"Harvested {result.plantName}!";

            NotificationManager.Instance.ShowNotification(message, NotificationType.Success);
        }

        return result;
    }

    private void OnTimeAdvanced(DateTime newTime)
    {
        if (IsEmpty) return;

        UpdatePlantGrowth();
        CheckWateringStatus();
    }

    private void OnHourPassed(int hour)
    {
        if (IsEmpty) return;

        // Check if watering effect has worn off
        if (isWatered && SimpleTimeManager.Instance.GetHoursPassedSince(lastWateredTime) >= 8f)
        {
            isWatered = false;
            UpdateVisuals();
        }
    }

    private void UpdatePlantGrowth()
    {
        if (currentPlant == null) return;

        Plant plantDef = currentPlant.plantData.plant;
        DateTime currentTime = SimpleTimeManager.Instance.CurrentTime;

        // Calculate total growth time needed
        float totalGrowthTime = plantDef.GetGrowthTimeSeconds();

        // Apply watering bonus
        if (isWatered)
        {
            totalGrowthTime /= plantDef.wateringBonus;
        }

        // Calculate growth progress
        float hoursGrown = SimpleTimeManager.Instance.GetHoursPassedSince(currentPlant.plantedTime);
        float growthProgress = (hoursGrown * 3600f) / totalGrowthTime; // Convert to 0-1 range

        // Determine current growth stage
        PlantGrowthStage newStage = CalculateGrowthStage(growthProgress);

        // Update if stage changed
        if (newStage != currentPlant.growthStage)
        {
            PlantGrowthStage previousStage = currentPlant.growthStage;
            currentPlant.growthStage = newStage;
            currentPlant.lastGrowthUpdate = currentTime;

            UpdateVisuals();
            OnPlantStageChanged?.Invoke(this, newStage);

            // Show growth notification
            if (newStage == PlantGrowthStage.Harvestable)
            {
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification(
                        $"{plantDef.displayName} is ready to harvest!",
                        NotificationType.Success
                    );
                }
            }
        }
    }

    private PlantGrowthStage CalculateGrowthStage(float progress)
    {
        if (progress >= 1f) return PlantGrowthStage.Harvestable;
        if (progress >= 0.8f) return PlantGrowthStage.Mature;
        if (progress >= 0.5f) return PlantGrowthStage.Growing;
        if (progress >= 0.2f) return PlantGrowthStage.Sprout;
        return PlantGrowthStage.Planted;
    }

    private void CheckWateringStatus()
    {
        // Visual update for watering needs
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        UpdatePlantSprite();
        UpdateSoilAppearance();
        UpdateWaterEffect();
    }

    private void UpdatePlantSprite()
    {
        if (plantRenderer == null) return;

        if (IsEmpty)
        {
            plantRenderer.sprite = null;
            plantRenderer.gameObject.SetActive(false);
            return;
        }

        plantRenderer.gameObject.SetActive(true);

        // Find matching visual for current growth stage
        Plant plantDef = currentPlant.plantData.plant;
        foreach (PlantStageVisuals visual in plantDef.stageVisuals)
        {
            if (visual.stage == currentPlant.growthStage)
            {
                plantRenderer.sprite = visual.sprite;
                transform.localScale = visual.scale;
                plantRenderer.color = visual.tint;
                return;
            }
        }

        // Fallback if no visual found
        plantRenderer.color = Color.white;
    }

    private void UpdateSoilAppearance()
    {
        if (soilRenderer == null) return;

        if (IsEmpty)
        {
            soilRenderer.color = Color.white; // Normal soil
        }
        else if (NeedsWatering)
        {
            soilRenderer.color = new Color(0.8f, 0.7f, 0.6f); // Dry soil (brownish)
        }
        else if (isWatered)
        {
            soilRenderer.color = new Color(0.6f, 0.4f, 0.3f); // Wet soil (darker brown)
        }
        else
        {
            soilRenderer.color = Color.white; // Normal soil
        }
    }

    private void UpdateWaterEffect()
    {
        if (waterDroplets != null)
        {
            waterDroplets.SetActive(isWatered);
        }
    }

    // Interaction detection
    public string GetInteractionPrompt()
    {
        if (CanHarvest())
            return "Press E to harvest";
        else if (CanWater())
            return "Press E to water";
        else if (CanPlant())
            return "Press E to plant";
        else
            return "";
    }

    public void OnPlayerInteract()
    {
        if (CanHarvest())
        {
            HarvestPlant();
        }
        else if (CanWater())
        {
            WaterPlant();
        }
        else if (CanPlant())
        {
            // This would open plant selection UI
            GardenManager.Instance?.ShowPlantSelectionUI(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

// Supporting classes
[System.Serializable]
public class PlantInstance
{
    public PlantData plantData;
    public DateTime plantedTime;
    public PlantGrowthStage growthStage;
    public int timesHarvested;
    public DateTime lastGrowthUpdate;
}

[System.Serializable]
public class HarvestResult
{
    public string itemId;
    public int quantity;
    public string plantName;
}
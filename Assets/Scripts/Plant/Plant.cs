using UnityEngine;

[System.Serializable]
public class Plant
{
    [Header("Basic Info")]
    public string plantId;
    public string displayName;
    [TextArea(2, 4)]
    public string description;

    [Header("Growth Settings")]
    public float growthTimeHours = 24f; // Real hours to fully grow
    public int maxHarvests = 3; // How many times can harvest before replanting
    public bool requiresWatering = true;
    public float wateringBonus = 1.5f; // Growth speed multiplier when watered

    [Header("Harvest Rewards")]
    public string harvestItemId; // What item it produces
    public int baseHarvestAmount = 1;
    public int maxHarvestAmount = 3;

    [Header("Visual")]
    public PlantStageVisuals[] stageVisuals; // Sprites for each growth stage

    public float GetGrowthTimeSeconds()
    {
        return growthTimeHours * 3600f; // Convert hours to seconds
    }
}

[System.Serializable]
public class PlantStageVisuals
{
    public PlantGrowthStage stage;
    public Sprite sprite;
    public Vector3 scale = Vector3.one;
    public Color tint = Color.white;
}

public enum PlantGrowthStage
{
    Empty = 0,      // No plant
    Planted = 1,    // Just planted seed
    Sprout = 2,     // Small sprout
    Growing = 3,    // Getting bigger
    Mature = 4,     // Almost ready
    Harvestable = 5 // Ready to harvest
}

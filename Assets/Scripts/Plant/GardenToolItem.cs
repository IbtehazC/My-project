// GardenToolItem.cs - Tools that can be used in garden (watering can, etc.)
using UnityEngine;

[CreateAssetMenu(fileName = "New Garden Tool", menuName = "Game/Garden Tool")]
public class GardenToolData : ScriptableObject
{
    [Header("Tool Info")]
    public string toolId;
    public string displayName;
    public string description;
    public Sprite icon;

    [Header("Tool Function")]
    public GardenToolType toolType;
    public float effectRadius = 1f; // For area-effect tools
    public int usesPerTool = 10; // Durability

    [Header("Effects")]
    public float wateringEfficiency = 1f; // How much water it provides
    public float harvestBonus = 1f; // Harvest amount multiplier
}

public enum GardenToolType
{
    WateringCan,
    Hoe,
    Fertilizer,
    Pesticide
}

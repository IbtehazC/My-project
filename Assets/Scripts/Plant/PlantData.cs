using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "Game/Plant")]
public class PlantData : ScriptableObject
{
    public Plant plant;

    private void OnValidate()
    {
        // Auto-generate ID from asset name if empty
        if (string.IsNullOrEmpty(plant.plantId))
        {
            plant.plantId = name.ToLower().Replace(" ", "_");
        }

        // Ensure display name is set
        if (string.IsNullOrEmpty(plant.displayName))
        {
            plant.displayName = name;
        }
    }
}
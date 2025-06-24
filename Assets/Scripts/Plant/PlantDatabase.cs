using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "PlantDatabase", menuName = "Game/Plant Database")]
public class PlantDatabase : ScriptableObject
{
    [Header("Plants")]
    [SerializeField] private List<PlantData> plants = new List<PlantData>();

    private Dictionary<string, PlantData> plantLookup;

    private void OnEnable()
    {
        BuildLookupDictionary();
    }

    private void BuildLookupDictionary()
    {
        plantLookup = new Dictionary<string, PlantData>();

        foreach (PlantData plantData in plants)
        {
            if (plantData != null && !string.IsNullOrEmpty(plantData.plant.plantId))
            {
                if (!plantLookup.ContainsKey(plantData.plant.plantId))
                {
                    plantLookup[plantData.plant.plantId] = plantData;
                }
                else
                {
                    Debug.LogWarning($"Duplicate plant ID found: {plantData.plant.plantId}");
                }
            }
        }
    }

    public PlantData GetPlant(string plantId)
    {
        if (plantLookup == null)
        {
            BuildLookupDictionary();
        }

        plantLookup.TryGetValue(plantId, out PlantData plantData);
        return plantData;
    }

    public List<PlantData> GetAllPlants()
    {
        return new List<PlantData>(plants);
    }

    public bool PlantExists(string plantId)
    {
        if (plantLookup == null)
        {
            BuildLookupDictionary();
        }

        return plantLookup.ContainsKey(plantId);
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Lookup Dictionary")]
    public void RebuildLookup()
    {
        BuildLookupDictionary();
    }
#endif
}

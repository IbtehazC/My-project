using UnityEngine;

public class GardenPlotInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 1.5f, 0);
    
    private GardenPlot gardenPlot;
    private bool playerInRange = false;
    
    private void Awake()
    {
        gardenPlot = GetComponent<GardenPlot>();
        if (gardenPlot == null)
        {
            Debug.LogError("GardenPlotInteractable requires a GardenPlot component!");
        }
    }
    
    public bool CanInteract()
    {
        if (gardenPlot == null) return false;
        
        return gardenPlot.CanPlant() || gardenPlot.CanWater() || gardenPlot.CanHarvest();
    }
    
    public void Interact()
    {
        if (gardenPlot == null) return;
        
        gardenPlot.OnPlayerInteract();
    }
    
    public void OnInteractionEnter()
    {
        playerInRange = true;
        // The interaction prompt will be handled by world-space prompts if implemented
    }
    
    public void OnInteractionExit()
    {
        playerInRange = false;
    }
    
    public string GetInteractionPrompt()
    {
        if (gardenPlot == null) return "";
        
        return gardenPlot.GetInteractionPrompt();
    }
    
    public KeyCode GetInteractionKey()
    {
        return interactionKey;
    }
    
    public Vector3 GetPromptOffset()
    {
        return promptOffset;
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public Item item;
    
    private void OnValidate()
    {
        // Auto-generate ID from asset name if empty
        if (string.IsNullOrEmpty(item.id))
        {
            item.id = name.ToLower().Replace(" ", "_");
        }
        
        // Ensure display name is set
        if (string.IsNullOrEmpty(item.displayName))
        {
            item.displayName = name;
        }
    }
}
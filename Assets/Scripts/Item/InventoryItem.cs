using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public int quantity;
    public ItemData itemData;
    
    // Additional instance data
    public float durability = 100f; // For tools
    public System.DateTime acquiredDate;
    public bool isFavorited = false;
    
    public InventoryItem(string id, int qty = 1)
    {
        itemId = id;
        quantity = qty;
        acquiredDate = System.DateTime.Now;
        
        // Get item data from database
        itemData = ItemManager.Instance?.GetItemData(id);
    }
    
    public InventoryItem(ItemData data, int qty = 1)
    {
        itemData = data;
        itemId = data?.item.id ?? "";
        quantity = qty;
        acquiredDate = System.DateTime.Now;
    }
    
    public bool CanStackWith(InventoryItem other)
    {
        if (other == null || itemData == null || other.itemData == null)
            return false;
            
        return itemId == other.itemId && 
               itemData.item.isStackable && 
               quantity + other.quantity <= itemData.item.maxStackSize;
    }
    
    public void AddQuantity(int amount)
    {
        if (itemData != null)
        {
            quantity = Mathf.Min(quantity + amount, itemData.item.maxStackSize);
        }
        else
        {
            quantity += amount;
        }
    }
    
    public bool RemoveQuantity(int amount)
    {
        if (quantity >= amount)
        {
            quantity -= amount;
            return true;
        }
        return false;
    }
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(itemId) && quantity > 0 && itemData != null;
    }
}
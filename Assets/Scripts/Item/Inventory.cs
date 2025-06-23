using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private bool unlimitedCapacity = true;
    [SerializeField] private int maxSlots = 999;
    
    [Header("Categories")]
    [SerializeField] private bool useCategories = true;
    
    // Inventory storage
    private List<InventoryItem> items = new List<InventoryItem>();
    private Dictionary<ItemType, List<InventoryItem>> categorizedItems = new Dictionary<ItemType, List<InventoryItem>>();
    
    // Events
    public Action<InventoryItem> OnItemAdded;
    public Action<InventoryItem> OnItemRemoved;
    public Action<InventoryItem> OnItemQuantityChanged;
    public Action OnInventoryChanged;
    
    public static Inventory Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeCategories();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeCategories()
    {
        if (useCategories)
        {
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                categorizedItems[itemType] = new List<InventoryItem>();
            }
        }
    }
    
    public bool AddItem(string itemId, int quantity = 1)
    {
        ItemData itemData = ItemManager.Instance?.GetItemData(itemId);
        if (itemData == null)
        {
            Debug.LogWarning($"Item not found: {itemId}");
            return false;
        }
        
        return AddItem(itemData, quantity);
    }
    
    public bool AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return false;
        
        // Check if item is stackable and we already have some
        if (itemData.item.isStackable)
        {
            InventoryItem existingItem = FindItem(itemData.item.id);
            if (existingItem != null)
            {
                int remainingCapacity = itemData.item.maxStackSize - existingItem.quantity;
                int amountToAdd = Mathf.Min(quantity, remainingCapacity);
                
                if (amountToAdd > 0)
                {
                    existingItem.AddQuantity(amountToAdd);
                    OnItemQuantityChanged?.Invoke(existingItem);
                    OnInventoryChanged?.Invoke();
                    
                    quantity -= amountToAdd;
                }
            }
        }
        
        // Add remaining quantity as new item(s)
        while (quantity > 0)
        {
            if (!unlimitedCapacity && items.Count >= maxSlots)
            {
                Debug.LogWarning("Inventory is full!");
                return false;
            }
            
            int stackSize = itemData.item.isStackable ? 
                           Mathf.Min(quantity, itemData.item.maxStackSize) : 1;
            
            InventoryItem newItem = new InventoryItem(itemData, stackSize);
            items.Add(newItem);
            
            if (useCategories)
            {
                categorizedItems[itemData.item.itemType].Add(newItem);
            }
            
            OnItemAdded?.Invoke(newItem);
            quantity -= stackSize;
        }
        
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    public bool RemoveItem(string itemId, int quantity = 1)
    {
        if (!HasItem(itemId, quantity)) return false;
        
        List<InventoryItem> itemsToModify = items.Where(item => item.itemId == itemId).ToList();
        int remainingToRemove = quantity;
        
        foreach (InventoryItem item in itemsToModify)
        {
            if (remainingToRemove <= 0) break;
            
            int amountToRemove = Mathf.Min(remainingToRemove, item.quantity);
            
            if (item.RemoveQuantity(amountToRemove))
            {
                remainingToRemove -= amountToRemove;
                
                if (item.quantity <= 0)
                {
                    RemoveItemFromInventory(item);
                }
                else
                {
                    OnItemQuantityChanged?.Invoke(item);
                }
            }
        }
        
        OnInventoryChanged?.Invoke();
        return remainingToRemove == 0;
    }
    
    private void RemoveItemFromInventory(InventoryItem item)
    {
        items.Remove(item);
        
        if (useCategories && item.itemData != null)
        {
            categorizedItems[item.itemData.item.itemType].Remove(item);
        }
        
        OnItemRemoved?.Invoke(item);
    }
    
    public bool HasItem(string itemId, int quantity = 1)
    {
        return GetItemCount(itemId) >= quantity;
    }
    
    public int GetItemCount(string itemId)
    {
        return items.Where(item => item.itemId == itemId).Sum(item => item.quantity);
    }
    
    public InventoryItem FindItem(string itemId)
    {
        return items.FirstOrDefault(item => item.itemId == itemId);
    }
    
    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(items);
    }
    
    public List<InventoryItem> GetItemsByType(ItemType itemType)
    {
        if (useCategories)
        {
            return new List<InventoryItem>(categorizedItems[itemType]);
        }
        
        return items.Where(item => item.itemData?.item.itemType == itemType).ToList();
    }
    
    public List<InventoryItem> GetItemsByRarity(ItemRarity rarity)
    {
        return items.Where(item => item.itemData?.item.rarity == rarity).ToList();
    }
    
    public bool UseItem(string itemId)
    {
        InventoryItem item = FindItem(itemId);
        if (item?.itemData?.item.CanUse() == true)
        {
            // Apply item effects
            ApplyItemEffects(item.itemData.item);
            
            // Remove item if consumable
            if (item.itemData.item.isConsumable)
            {
                RemoveItem(itemId, 1);
            }
            
            return true;
        }
        
        return false;
    }
    
    private void ApplyItemEffects(Item item)
    {
        foreach (ItemEffect effect in item.effects)
        {
            switch (effect.effectType)
            {
                case ItemEffectType.RestoreHealth:
                    // Apply health restoration
                    Debug.Log($"Restored {effect.value} health");
                    break;
                    
                case ItemEffectType.RestoreEnergy:
                    // Apply energy restoration
                    Debug.Log($"Restored {effect.value} energy");
                    break;
                    
                // Add more effect types as needed
            }
        }
    }
    
    public void SortInventory(InventorySortType sortType = InventorySortType.Type)
    {
        switch (sortType)
        {
            case InventorySortType.Type:
                items = items.OrderBy(item => item.itemData?.item.itemType).ToList();
                break;
                
            case InventorySortType.Rarity:
                items = items.OrderByDescending(item => item.itemData?.item.rarity).ToList();
                break;
                
            case InventorySortType.Name:
                items = items.OrderBy(item => item.itemData?.item.displayName).ToList();
                break;
                
            case InventorySortType.Quantity:
                items = items.OrderByDescending(item => item.quantity).ToList();
                break;
                
            case InventorySortType.Value:
                items = items.OrderByDescending(item => item.itemData?.item.GetValue()).ToList();
                break;
        }
        
        OnInventoryChanged?.Invoke();
    }
    
    public int GetTotalValue()
    {
        return items.Sum(item => (item.itemData?.item.GetValue() ?? 0) * item.quantity);
    }
    
    public void ClearInventory()
    {
        items.Clear();
        
        if (useCategories)
        {
            foreach (var category in categorizedItems.Values)
            {
                category.Clear();
            }
        }
        
        OnInventoryChanged?.Invoke();
    }
}

public enum InventorySortType
{
    Type,
    Rarity,
    Name,
    Quantity,
    Value
}
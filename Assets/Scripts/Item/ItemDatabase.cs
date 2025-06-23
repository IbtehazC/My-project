using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("Items")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    
    // Dictionary for fast lookup
    private Dictionary<string, ItemData> itemLookup;
    
    private void OnEnable()
    {
        BuildLookupDictionary();
    }
    
    private void BuildLookupDictionary()
    {
        itemLookup = new Dictionary<string, ItemData>();
        
        foreach (ItemData itemData in items)
        {
            if (itemData != null && !string.IsNullOrEmpty(itemData.item.id))
            {
                if (!itemLookup.ContainsKey(itemData.item.id))
                {
                    itemLookup[itemData.item.id] = itemData;
                }
                else
                {
                    Debug.LogWarning($"Duplicate item ID found: {itemData.item.id}");
                }
            }
        }
    }
    
    public ItemData GetItem(string itemId)
    {
        if (itemLookup == null)
        {
            BuildLookupDictionary();
        }
        
        itemLookup.TryGetValue(itemId, out ItemData itemData);
        return itemData;
    }
    
    public List<ItemData> GetItemsByType(ItemType itemType)
    {
        return items.Where(item => item.item.itemType == itemType).ToList();
    }
    
    public List<ItemData> GetItemsByRarity(ItemRarity rarity)
    {
        return items.Where(item => item.item.rarity == rarity).ToList();
    }
    
    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(items);
    }
    
    public bool ItemExists(string itemId)
    {
        if (itemLookup == null)
        {
            BuildLookupDictionary();
        }
        
        return itemLookup.ContainsKey(itemId);
    }
    
    // Editor methods
    #if UNITY_EDITOR
    public void AddItem(ItemData itemData)
    {
        if (itemData != null && !items.Contains(itemData))
        {
            items.Add(itemData);
            BuildLookupDictionary();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
    
    public void RemoveItem(ItemData itemData)
    {
        if (items.Remove(itemData))
        {
            BuildLookupDictionary();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
    
    [ContextMenu("Rebuild Lookup Dictionary")]
    public void RebuildLookup()
    {
        BuildLookupDictionary();
    }
    #endif
}
using UnityEngine;

[System.Serializable]
public class Item
{
    [Header("Basic Info")]
    public string id;
    public string displayName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    
    [Header("Item Properties")]
    public ItemType itemType;
    public ItemRarity rarity;
    public bool isStackable = true;
    public int maxStackSize = 999;
    public int baseValue = 1;
    
    [Header("Usage")]
    public bool isUsable = false;
    public bool isConsumable = false;
    public ItemUseType useType = ItemUseType.None;
    
    [Header("Effects")]
    public ItemEffect[] effects;
    
    // Constructor
    public Item()
    {
        id = "";
        displayName = "";
        description = "";
        itemType = ItemType.Misc;
        rarity = ItemRarity.Common;
    }
    
    public Item(string itemId, string name, string desc, ItemType type)
    {
        id = itemId;
        displayName = name;
        description = desc;
        itemType = type;
        rarity = ItemRarity.Common;
    }
    
    // Get item value based on rarity
    public int GetValue()
    {
        float multiplier = rarity switch
        {
            ItemRarity.Common => 1f,
            ItemRarity.Uncommon => 1.5f,
            ItemRarity.Rare => 2f,
            ItemRarity.Epic => 3f,
            ItemRarity.Legendary => 5f,
            _ => 1f
        };
        
        return Mathf.RoundToInt(baseValue * multiplier);
    }
    
    // Get rarity color
    public Color GetRarityColor()
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => Color.green,
            ItemRarity.Rare => Color.blue,
            ItemRarity.Epic => new Color(0.6f, 0.2f, 0.8f), // Purple
            ItemRarity.Legendary => Color.yellow,
            _ => Color.white
        };
    }
    
    public bool CanUse()
    {
        return isUsable && useType != ItemUseType.None;
    }
}

[System.Serializable]
public class ItemEffect
{
    public ItemEffectType effectType;
    public float value;
    public float duration;
    public string description;
}

public enum ItemType
{
    Herb,
    CookedFood,
    RawFood,
    Tool,
    QuestItem,
    Material,
    Misc
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum ItemUseType
{
    None,
    Consume,
    Equip,
    Use,
    Give
}

public enum ItemEffectType
{
    None,
    RestoreHealth,
    RestoreEnergy,
    Buff,
    Debuff
}
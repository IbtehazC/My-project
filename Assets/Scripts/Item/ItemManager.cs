using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("Item Database")]
    [SerializeField] private ItemDatabase itemDatabase;

    public static ItemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ItemData GetItemData(string itemId)
    {
        if (itemDatabase != null)
        {
            return itemDatabase.GetItem(itemId);
        }

        Debug.LogWarning("Item database not assigned!");
        return null;
    }

    public bool ItemExists(string itemId)
    {
        return itemDatabase?.ItemExists(itemId) ?? false;
    }

    public void SetItemDatabase(ItemDatabase database)
    {
        itemDatabase = database;
    }

    // Convenience methods
    public static bool TryGetItem(string itemId, out ItemData itemData)
    {
        itemData = Instance?.GetItemData(itemId);
        return itemData != null;
    }

    public static void GivePlayerItem(string itemId, int quantity = 1)
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(itemId, quantity);

            // Show notification
            ItemData itemData = Instance?.GetItemData(itemId);
            if (itemData != null && NotificationManager.Instance != null)
            {
                string message = quantity > 1 ?
                    $"Received {quantity}x {itemData.item.displayName}" :
                    $"Received {itemData.item.displayName}";

                NotificationManager.Instance.ShowNotification(message, NotificationType.Success);
            }
        }
    }
}
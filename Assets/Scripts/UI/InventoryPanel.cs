using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InventoryPanel : UIPanel
{
    [Header("Inventory UI References")]
    [SerializeField] private Transform itemGridContent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private TMP_Dropdown sortDropdown;
    [SerializeField] private Button closeButton;

    [Header("Category Tabs")]
    [SerializeField] private Toggle allTab;
    [SerializeField] private Toggle herbsTab;
    [SerializeField] private Toggle foodTab;
    [SerializeField] private Toggle toolsTab;
    [SerializeField] private Toggle miscTab;

    [Header("Item Detail Panel")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private Image detailItemIcon;
    [SerializeField] private TextMeshProUGUI detailItemName;
    [SerializeField] private TextMeshProUGUI detailItemDescription;
    [SerializeField] private TextMeshProUGUI detailItemStats;
    [SerializeField] private Button useButton;
    [SerializeField] private Button closeDetailButton;

    [Header("Footer Info")]
    [SerializeField] private TextMeshProUGUI totalSlotsText;
    [SerializeField] private TextMeshProUGUI totalValueText;

    [Header("Settings")]
    [SerializeField] private int slotsPerRow = 8;
    [SerializeField] private bool useObjectPooling = true;

    // Current state
    private ItemType currentFilter = ItemType.Misc; // "All" category
    private InventorySortType currentSort = InventorySortType.Type;
    private InventoryItem selectedItem;
    private List<InventoryItemSlot> itemSlots = new List<InventoryItemSlot>();

    // Object pooling
    private Queue<GameObject> slotPool = new Queue<GameObject>();

    protected override void OnShowComplete()
    {
        RefreshInventory();
        SetupEventListeners();
        HideItemDetail();
    }

    protected override void OnHideComplete()
    {
        RemoveEventListeners();
        HideItemDetail();
    }

    private void SetupEventListeners()
    {
        // Inventory events
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged += OnInventoryChanged;
        }

        // UI events
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseInventory);

        if (sortDropdown != null)
            sortDropdown.onValueChanged.AddListener(OnSortChanged);

        if (closeDetailButton != null)
            closeDetailButton.onClick.AddListener(HideItemDetail);

        if (useButton != null)
            useButton.onClick.AddListener(UseSelectedItem);

        // Category tabs
        SetupCategoryTabs();
    }

    private void RemoveEventListeners()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged -= OnInventoryChanged;
        }

        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        if (sortDropdown != null)
            sortDropdown.onValueChanged.RemoveAllListeners();

        if (closeDetailButton != null)
            closeDetailButton.onClick.RemoveAllListeners();

        if (useButton != null)
            useButton.onClick.RemoveAllListeners();
    }

    // Event handler that matches the Action signature
    private void OnInventoryChanged()
    {
        RefreshInventory();
    }

    private void SetupCategoryTabs()
    {
        if (allTab != null)
            allTab.onValueChanged.AddListener((isOn) => { if (isOn) SetFilter("All"); });

        if (herbsTab != null)
            herbsTab.onValueChanged.AddListener((isOn) => { if (isOn) SetFilter(ItemType.Herb); });

        if (foodTab != null)
            foodTab.onValueChanged.AddListener((isOn) => { if (isOn) SetFilter(ItemType.CookedFood); });

        if (toolsTab != null)
            toolsTab.onValueChanged.AddListener((isOn) => { if (isOn) SetFilter(ItemType.Tool); });

        if (miscTab != null)
            miscTab.onValueChanged.AddListener((isOn) => { if (isOn) SetFilter(ItemType.Misc); });
    }

    private void SetFilter(object filter)
    {
        if (filter is string && (string)filter == "All")
        {
            currentFilter = ItemType.Misc; // Use as "All" placeholder
            RefreshInventory(showAll: true);
        }
        else if (filter is ItemType)
        {
            currentFilter = (ItemType)filter;
            RefreshInventory(showAll: false);
        }
    }

    private void OnSortChanged(int sortIndex)
    {
        currentSort = (InventorySortType)sortIndex;
        RefreshInventory();
    }

    public void RefreshInventory(bool showAll = false)
    {
        if (Inventory.Instance == null) return;

        // Get items based on current filter
        List<InventoryItem> itemsToShow;

        if (showAll || allTab?.isOn == true)
        {
            itemsToShow = Inventory.Instance.GetAllItems();
        }
        else
        {
            itemsToShow = Inventory.Instance.GetItemsByType(currentFilter);
        }

        // Sort items
        itemsToShow = SortItems(itemsToShow);

        // Clear existing slots
        ClearItemSlots();

        // Create slots for items
        CreateItemSlots(itemsToShow);

        // Update footer info
        UpdateFooterInfo();
    }

    private List<InventoryItem> SortItems(List<InventoryItem> items)
    {
        switch (currentSort)
        {
            case InventorySortType.Type:
                return items.OrderBy(item => item.itemData?.item.itemType).ToList();

            case InventorySortType.Rarity:
                return items.OrderByDescending(item => item.itemData?.item.rarity).ToList();

            case InventorySortType.Name:
                return items.OrderBy(item => item.itemData?.item.displayName).ToList();

            case InventorySortType.Quantity:
                return items.OrderByDescending(item => item.quantity).ToList();

            case InventorySortType.Value:
                return items.OrderByDescending(item => item.itemData?.item.GetValue() * item.quantity).ToList();

            default:
                return items;
        }
    }

    private void ClearItemSlots()
    {
        foreach (InventoryItemSlot slot in itemSlots)
        {
            if (useObjectPooling)
            {
                slot.gameObject.SetActive(false);
                slotPool.Enqueue(slot.gameObject);
            }
            else
            {
                Destroy(slot.gameObject);
            }
        }

        itemSlots.Clear();
    }

    private void CreateItemSlots(List<InventoryItem> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            InventoryItem item = items[i];
            GameObject slotObj = GetOrCreateSlot();

            InventoryItemSlot slot = slotObj.GetComponent<InventoryItemSlot>();
            if (slot == null)
            {
                slot = slotObj.AddComponent<InventoryItemSlot>();
            }

            slot.Initialize(item, this);
            itemSlots.Add(slot);

            slotObj.SetActive(true);
        }
    }

    private GameObject GetOrCreateSlot()
    {
        if (useObjectPooling && slotPool.Count > 0)
        {
            return slotPool.Dequeue();
        }
        else
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, itemGridContent);
            return newSlot;
        }
    }

    public void OnItemSlotClicked(InventoryItem item)
    {
        selectedItem = item;
        ShowItemDetail(item);
    }

    private void ShowItemDetail(InventoryItem item)
    {
        if (itemDetailPanel == null || item?.itemData == null) return;

        itemDetailPanel.SetActive(true);

        // Update detail panel
        if (detailItemIcon != null)
        {
            detailItemIcon.sprite = item.itemData.item.icon;
        }

        if (detailItemName != null)
        {
            detailItemName.text = item.itemData.item.displayName;
            detailItemName.color = item.itemData.item.GetRarityColor();
        }

        if (detailItemDescription != null)
        {
            detailItemDescription.text = item.itemData.item.description;
        }

        if (detailItemStats != null)
        {
            string stats = BuildItemStatsText(item);
            detailItemStats.text = stats;
        }

        // Update use button
        if (useButton != null)
        {
            useButton.gameObject.SetActive(item.itemData.item.CanUse());

            TextMeshProUGUI buttonText = useButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = GetUseButtonText(item.itemData.item);
            }
        }
    }

    private string BuildItemStatsText(InventoryItem item)
    {
        var stats = new System.Text.StringBuilder();

        stats.AppendLine($"Quantity: {item.quantity}");
        stats.AppendLine($"Value: ${item.itemData.item.GetValue():N0} each");
        stats.AppendLine($"Total Value: ${item.itemData.item.GetValue() * item.quantity:N0}");
        stats.AppendLine($"Type: {item.itemData.item.itemType}");
        stats.AppendLine($"Rarity: {item.itemData.item.rarity}");

        if (item.itemData.item.effects != null && item.itemData.item.effects.Length > 0)
        {
            stats.AppendLine("\nEffects:");
            foreach (var effect in item.itemData.item.effects)
            {
                stats.AppendLine($"• {effect.description}");
            }
        }

        return stats.ToString();
    }

    private string GetUseButtonText(Item item)
    {
        switch (item.useType)
        {
            case ItemUseType.Consume:
                return "Consume";
            case ItemUseType.Equip:
                return "Equip";
            case ItemUseType.Use:
                return "Use";
            case ItemUseType.Give:
                return "Give";
            default:
                return "Use";
        }
    }

    private void UseSelectedItem()
    {
        if (selectedItem != null && Inventory.Instance != null)
        {
            bool success = Inventory.Instance.UseItem(selectedItem.itemId);

            if (success)
            {
                // Show notification
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification(
                        $"Used {selectedItem.itemData.item.displayName}",
                        NotificationType.Success
                    );
                }

                // Hide detail panel if item was consumed
                if (selectedItem.itemData.item.isConsumable)
                {
                    HideItemDetail();
                }
            }
        }
    }

    private void HideItemDetail()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
        }

        selectedItem = null;
    }

    private void UpdateFooterInfo()
    {
        if (Inventory.Instance == null) return;

        if (totalSlotsText != null)
        {
            int totalItems = Inventory.Instance.GetAllItems().Count;
            totalSlotsText.text = $"{totalItems} items";
        }

        if (totalValueText != null)
        {
            int totalValue = Inventory.Instance.GetTotalValue();
            totalValueText.text = $"Total Value: ${totalValue:N0}";
        }
    }

    private void CloseInventory()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClosePanel(UIPanelType.Inventory);
        }
    }

    // Public methods for external use
    public void SortInventory(InventorySortType sortType)
    {
        currentSort = sortType;
        if (sortDropdown != null)
        {
            sortDropdown.value = (int)sortType;
        }
        RefreshInventory();
    }

    public void FilterByType(ItemType itemType)
    {
        currentFilter = itemType;
        RefreshInventory();
    }

    public void ShowAllItems()
    {
        RefreshInventory(showAll: true);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private Button button;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.cyan;
    [SerializeField] private float hoverScale = 1.1f;
    
    private InventoryItem currentItem;
    private InventoryPanel parentPanel;
    private bool isSelected = false;
    private bool isHovered = false;
    private Vector3 originalScale;
    
    private void Awake()
    {
        originalScale = transform.localScale;
        
        // Setup button if not assigned
        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }
        }
        
        // Setup default state
        SetEmpty();
    }
    
    public void Initialize(InventoryItem item, InventoryPanel panel)
    {
        currentItem = item;
        parentPanel = panel;
        
        UpdateSlotDisplay();
    }
    
    private void UpdateSlotDisplay()
    {
        if (currentItem?.itemData == null)
        {
            SetEmpty();
            return;
        }
        
        // Update item icon
        if (itemIcon != null)
        {
            itemIcon.sprite = currentItem.itemData.item.icon;
            itemIcon.gameObject.SetActive(true);
        }
        
        // Update quantity text
        if (quantityText != null)
        {
            if (currentItem.quantity > 1)
            {
                quantityText.text = currentItem.quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }
        
        // Update rarity border
        if (rarityBorder != null)
        {
            rarityBorder.color = currentItem.itemData.item.GetRarityColor();
            rarityBorder.gameObject.SetActive(true);
        }
        
        // Update background
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
        
        // Enable interaction
        if (button != null)
        {
            button.interactable = true;
        }
    }
    
    private void SetEmpty()
    {
        // Hide all visual elements for empty slot
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.gameObject.SetActive(false);
        }
        
        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(false);
        }
        
        if (rarityBorder != null)
        {
            rarityBorder.gameObject.SetActive(false);
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.clear;
        }
        
        if (button != null)
        {
            button.interactable = false;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem != null && parentPanel != null)
        {
            parentPanel.OnItemSlotClicked(currentItem);
            SetSelected(true);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            isHovered = true;
            UpdateVisualState();
            
            // Optional: Show quick tooltip
            ShowQuickTooltip();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisualState();
        
        // Hide quick tooltip
        HideQuickTooltip();
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisualState();
    }
    
    private void UpdateVisualState()
    {
        if (backgroundImage == null) return;
        
        Color targetColor;
        float targetScale = 1f;
        
        if (isSelected)
        {
            targetColor = selectedColor;
            targetScale = hoverScale;
        }
        else if (isHovered)
        {
            targetColor = highlightColor;
            targetScale = hoverScale;
        }
        else
        {
            targetColor = normalColor;
            targetScale = 1f;
        }
        
        backgroundImage.color = targetColor;
        transform.localScale = originalScale * targetScale;
    }
    
    private void ShowQuickTooltip()
    {
        // Optional: Implement quick tooltip showing item name and value
        // This could be a simple text popup near the cursor
        if (currentItem?.itemData != null)
        {
            // You could implement a tooltip system here
            // TooltipManager.Instance?.ShowTooltip(currentItem.itemData.item.displayName);
        }
    }
    
    private void HideQuickTooltip()
    {
        // Hide the quick tooltip
        // TooltipManager.Instance?.HideTooltip();
    }
    
    // Context menu support (right-click actions)
    public void OnPointerClick_ContextMenu(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && currentItem != null)
        {
            ShowContextMenu();
        }
    }
    
    private void ShowContextMenu()
    {
        // Optional: Show context menu with actions like Use, Drop, Split Stack, etc.
        Debug.Log($"Context menu for {currentItem.itemData.item.displayName}");
        
        // You could implement a context menu system here
        // ContextMenuManager.Instance?.ShowContextMenu(currentItem, transform.position);
    }
    
    // Animation methods
    private void AnimatePickup()
    {
        // Optional: Animate when item is added to slot
        StartCoroutine(AnimateScale(Vector3.zero, originalScale, 0.2f));
    }
    
    private void AnimateRemoval()
    {
        // Optional: Animate when item is removed from slot
        StartCoroutine(AnimateScale(originalScale, Vector3.zero, 0.2f));
    }
    
    private System.Collections.IEnumerator AnimateScale(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            
            yield return null;
        }
        
        transform.localScale = endScale;
    }
    
    // Utility methods
    public bool IsEmpty()
    {
        return currentItem == null;
    }
    
    public InventoryItem GetItem()
    {
        return currentItem;
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        SetEmpty();
    }
    
    // Drag and drop support (optional for later)
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Implement drag and drop functionality
        // This would allow players to drag items between slots
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Update drag visual
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Handle drop
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // Handle item dropped on this slot
    }
}
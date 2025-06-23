using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    [Header("Prompt Settings")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image keyIcon;
    [SerializeField] private Image backgroundImage;

    [Header("Animation Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    [Header("Prompt Appearance")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Transform target;
    private Camera playerCamera;
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    private bool isHighlighted = false;
    private Vector3 initialPosition;
    private float bobTimer = 0f;

    private void Awake()
    {
        // Get components
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Find player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }

        // Start hidden
        canvasGroup.alpha = 0f;
        promptPanel.SetActive(false);

        initialPosition = transform.localPosition;
    }

    private void Update()
    {
        if (target != null)
        {
            UpdatePosition();
            UpdateBobAnimation();
            UpdateFacing();
        }

        UpdateVisibility();
    }

    public void Initialize(Transform targetObject, string promptMessage, KeyCode interactionKey = KeyCode.E)
    {
        target = targetObject;

        if (promptText != null)
        {
            promptText.text = promptMessage;
        }

        // Set key icon (you'd need to have key sprites for this)
        UpdateKeyIcon(interactionKey);

        // Position at target
        UpdatePosition();
    }

    public void ShowPrompt(string message = "")
    {
        if (!string.IsNullOrEmpty(message) && promptText != null)
        {
            promptText.text = message;
        }

        isVisible = true;
        promptPanel.SetActive(true);
    }

    public void HidePrompt()
    {
        isVisible = false;
    }

    public void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;

        if (backgroundImage != null)
        {
            backgroundImage.color = highlighted ? highlightColor : normalColor;
        }
    }

    private void UpdatePosition()
    {
        if (target == null || playerCamera == null) return;

        // Get world position with offset
        Vector3 worldPosition = target.position + offset;

        // Convert to screen space
        Vector3 screenPosition = playerCamera.WorldToScreenPoint(worldPosition);

        // Check if behind camera
        if (screenPosition.z < 0)
        {
            isVisible = false;
            return;
        }

        // Update UI position
        transform.position = screenPosition;
    }

    private void UpdateBobAnimation()
    {
        if (!isVisible) return;

        bobTimer += Time.deltaTime * bobSpeed;
        float bobOffset = Mathf.Sin(bobTimer) * bobHeight;

        Vector3 currentPos = transform.position;
        currentPos.y = playerCamera.WorldToScreenPoint(target.position + offset).y + bobOffset * 100f; // Scale for screen space
        transform.position = currentPos;
    }

    private void UpdateFacing()
    {
        // Always face the camera (billboard effect)
        if (playerCamera != null)
        {
            Vector3 lookDirection = playerCamera.transform.position - transform.position;
            lookDirection.z = 0; // Keep in 2D plane
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            }
        }
    }

    private void UpdateVisibility()
    {
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        if (canvasGroup.alpha < 0.01f && !isVisible)
        {
            promptPanel.SetActive(false);
        }
    }

    private void UpdateKeyIcon(KeyCode key)
    {
        if (keyIcon != null)
        {
            // You would set different sprites based on the key
            // For now, just update the text if you don't have key sprites
            if (promptText != null)
            {
                string keyText = key.ToString();
                promptText.text = promptText.text.Replace("{KEY}", keyText);
            }
        }
    }

    public void SetDistance(float distance)
    {
        // Optional: Scale prompt based on distance
        float scale = Mathf.Clamp(1f - (distance / 10f), 0.5f, 1f);
        transform.localScale = Vector3.one * scale;
    }
}
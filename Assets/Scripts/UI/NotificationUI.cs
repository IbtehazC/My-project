using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NotificationUI : MonoBehaviour
{
    [Header("Notification Components")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button closeButton;

    [Header("Colors")]
    [SerializeField] private Color infoColor = Color.blue;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color errorColor = Color.red;

    private RectTransform rectTransform;
    private bool isClosing = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ForceClose);
        }
    }

    public void Initialize(string message, NotificationType type, float duration)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        SetNotificationColor(type);
        StartCoroutine(SlideIn());
    }

    private void SetNotificationColor(NotificationType type)
    {
        if (backgroundImage != null)
        {
            Color color = type switch
            {
                NotificationType.Success => successColor,
                NotificationType.Warning => warningColor,
                NotificationType.Error => errorColor,
                _ => infoColor
            };

            backgroundImage.color = color;
        }
    }

    private IEnumerator SlideIn()
    {
        Vector2 startPos = new Vector2(Screen.width + rectTransform.rect.width, rectTransform.anchoredPosition.y);
        Vector2 endPos = rectTransform.anchoredPosition;

        rectTransform.anchoredPosition = startPos;

        float elapsedTime = 0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
    }

    public void Close()
    {
        if (!isClosing)
        {
            StartCoroutine(SlideOut());
        }
    }

    public void ForceClose()
    {
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.CloseNotification(this);
        }
    }

    private IEnumerator SlideOut()
    {
        isClosing = true;

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = new Vector2(Screen.width + rectTransform.rect.width, rectTransform.anchoredPosition.y);

        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            yield return null;
        }

        Destroy(gameObject);
    }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}
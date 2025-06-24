using UnityEngine;
using System.Collections;

public class UIPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private bool startVisible = false;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private bool useAnimations = true;

    public UIPanelType PanelType { get; private set; }
    public bool IsVisible { get; private set; }
    public bool IsTransitioning { get; private set; } // New: track animation state

    private CanvasGroup canvasGroup;
    private Coroutine currentTransition; // Track current animation coroutine

    private void Awake()
    {
        SetupComponents();
    }

    private void SetupComponents()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (!startVisible)
        {
            SetVisibleImmediate(false);
        }
    }

    public void Initialize(UIPanelType panelType)
    {
        PanelType = panelType;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        // Ensure we can start coroutines after activating
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(AnimateShow());
        }
        else
        {
            // Fallback: Show immediately without animation
            SetVisibleImmediate(true);
        }
    }

    public void Hide()
    {
        StartCoroutine(AnimateHide());
    }

    private void StopCurrentTransition()
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
        }
        IsTransitioning = false;
    }

    private IEnumerator ShowCoroutine()
    {
        // Wait one frame to ensure GameObject is fully activated
        yield return null;
        yield return StartCoroutine(AnimateShow());

        // Animation complete
        IsTransitioning = false;
        currentTransition = null;
    }

    private IEnumerator AnimateShow()
    {
        IsVisible = true;
        yield return StartCoroutine(AnimateFade(0f, 1f));
        OnShowComplete();
    }

    private IEnumerator AnimateHide()
    {
        IsVisible = false;
        yield return StartCoroutine(AnimateFade(1f, 0f));
        gameObject.SetActive(false);
        OnHideComplete();

        // Animation complete
        IsTransitioning = false;
        currentTransition = null;
    }

    private IEnumerator AnimateFade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    private void SetVisibleImmediate(bool visible)
    {
        // Stop any ongoing transition
        StopCurrentTransition();

        IsVisible = visible;
        gameObject.SetActive(visible);

        if (visible)
        {
            canvasGroup.alpha = 1f;
            OnShowComplete();
        }
        else
        {
            canvasGroup.alpha = 0f;
            OnHideComplete();
        }
    }

    private void OnDestroy()
    {
        // Clean up any ongoing transitions
        StopCurrentTransition();
    }

    protected virtual void OnShowComplete()
    {
        // Override in derived classes
    }

    protected virtual void OnHideComplete()
    {
        // Override in derived classes
    }
}
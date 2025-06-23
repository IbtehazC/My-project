using UnityEngine;
using System.Collections;

public class UIPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private bool startVisible = false;
    [SerializeField] private float animationDuration = 0.3f;

    public UIPanelType PanelType { get; private set; }
    public bool IsVisible { get; private set; }

    private CanvasGroup canvasGroup;

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
        StartCoroutine(AnimateShow());
    }

    public void Hide()
    {
        StartCoroutine(AnimateHide());
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

    protected virtual void OnShowComplete()
    {
        // Override in derived classes
    }

    protected virtual void OnHideComplete()
    {
        // Override in derived classes
    }
}
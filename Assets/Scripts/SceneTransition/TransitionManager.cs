using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private Image fadeImage;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Text loadingText;
    [SerializeField] private Slider loadingBar;
    
    private void Awake()
    {
        // Ensure transition canvas exists
        if (transitionCanvas == null)
        {
            CreateTransitionCanvas();
        }
        
        // Start with clear screen
        SetFadeAlpha(0f);
    }
    
    private void CreateTransitionCanvas()
    {
        // Create canvas
        GameObject canvasGO = new GameObject("TransitionCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // High priority
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create fade image
        GameObject fadeGO = new GameObject("FadeImage");
        fadeGO.transform.SetParent(canvasGO.transform);
        
        fadeImage = fadeGO.AddComponent<Image>();
        fadeImage.color = Color.black;
        
        RectTransform fadeRect = fadeImage.rectTransform;
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        
        transitionCanvas = canvasGO;
        DontDestroyOnLoad(transitionCanvas);
    }
    
    public IEnumerator FadeOut(float duration)
    {
        transitionCanvas.SetActive(true);
        yield return StartCoroutine(FadeToAlpha(1f, duration));
    }
    
    public IEnumerator FadeIn(float duration)
    {
        yield return StartCoroutine(FadeToAlpha(0f, duration));
        transitionCanvas.SetActive(false);
    }
    
    private IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        float startAlpha = fadeImage.color.a;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;
            float easedProgress = fadeCurve.Evaluate(progress);
            
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, easedProgress);
            SetFadeAlpha(currentAlpha);
            
            yield return null;
        }
        
        SetFadeAlpha(targetAlpha);
    }
    
    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
    
    public void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(show);
        }
    }
    
    public void UpdateLoadingProgress(float progress, string text = "Loading...")
    {
        if (loadingBar != null)
        {
            loadingBar.value = progress;
        }
        
        if (loadingText != null)
        {
            loadingText.text = text;
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private bool showLoadingScreen = true;
    
    public static GameSceneManager Instance { get; private set; }
    
    [Header("Current Scene Info")]
    public string currentSceneName;
    public Vector3 playerSpawnPosition;
    public string playerSpawnDirection = "down";
    
    private bool isTransitioning = false;
    private TransitionManager transitionManager;
    private PersistentDataManager persistentData;
    
    // Events
    public System.Action<string> OnSceneLoadStarted;
    public System.Action<string> OnSceneLoadCompleted;
    public System.Action OnTransitionStarted;
    public System.Action OnTransitionCompleted;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSceneManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSceneManager()
    {
        transitionManager = GetComponent<TransitionManager>();
        persistentData = GetComponent<PersistentDataManager>();
        
        // Get current scene name
        currentSceneName = SceneManager.GetActiveScene().name;
        
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    public void LoadScene(string sceneName, Vector3 spawnPosition, string spawnDirection = "down")
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Scene transition already in progress!");
            return;
        }
        
        StartCoroutine(LoadSceneCoroutine(sceneName, spawnPosition, spawnDirection));
    }
    
    public void LoadScene(SceneTransitionData transitionData)
    {
        LoadScene(transitionData.targetScene, transitionData.playerSpawnPosition, transitionData.playerSpawnDirection);
    }
    
    private IEnumerator LoadSceneCoroutine(string sceneName, Vector3 spawnPosition, string spawnDirection)
    {
        isTransitioning = true;
        OnTransitionStarted?.Invoke();
        OnSceneLoadStarted?.Invoke(sceneName);
        
        // Store data before transition
        persistentData.StoreCurrentSceneData();
        
        // Start transition effect
        if (transitionManager != null)
        {
            yield return StartCoroutine(transitionManager.FadeOut(transitionDuration * 0.5f));
        }
        
        // Store spawn info for new scene
        playerSpawnPosition = spawnPosition;
        playerSpawnDirection = spawnDirection;
        
        // Load new scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                // Scene is ready, activate it
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
        
        // Scene loaded, wait a frame for initialization
        yield return null;
        
        // Position player
        PositionPlayerInNewScene();
        
        // Restore persistent data
        persistentData.RestoreSceneData(sceneName);
        
        // Complete transition
        if (transitionManager != null)
        {
            yield return StartCoroutine(transitionManager.FadeIn(transitionDuration * 0.5f));
        }
        
        currentSceneName = sceneName;
        isTransitioning = false;
        
        OnSceneLoadCompleted?.Invoke(sceneName);
        OnTransitionCompleted?.Invoke();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
    }
    
    private void PositionPlayerInNewScene()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = playerSpawnPosition;
            
            // Set player facing direction
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                SetPlayerDirection(playerController, playerSpawnDirection);
            }
        }
    }
    
    private void SetPlayerDirection(PlayerController playerController, string direction)
    {
        Vector2 directionVector = direction.ToLower() switch
        {
            "up" => Vector2.up,
            "down" => Vector2.down,
            "left" => Vector2.left,
            "right" => Vector2.right,
            "up-left" => new Vector2(-0.7f, 0.7f),
            "up-right" => new Vector2(0.7f, 0.7f),
            "down-left" => new Vector2(-0.7f, -0.7f),
            "down-right" => new Vector2(0.7f, -0.7f),
            _ => Vector2.down
        };
        
        // Set the last move direction for animation
        PlayerMovement movement = playerController.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            // This would require adding a SetLastDirection method to PlayerMovement
            // movement.SetLastDirection(directionVector);
        }
    }
    
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    public void ReloadCurrentScene()
    {
        LoadScene(currentSceneName, playerSpawnPosition, playerSpawnDirection);
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
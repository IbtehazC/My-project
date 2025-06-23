using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private Vector3 playerSpawnPosition;
    [SerializeField] private string playerSpawnDirection = "down";
    [SerializeField] private bool requireInteraction = false;
    
    [Header("Trigger Settings")]
    [SerializeField] private string requiredTag = "Player";
    
    private bool playerInRange = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(requiredTag))
        {
            playerInRange = true;
            
            if (!requireInteraction)
            {
                TriggerTransition();
            }
            else
            {
                // Show interaction prompt
                Debug.Log($"Press E to enter {targetSceneName}");
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(requiredTag))
        {
            playerInRange = false;
            // Hide interaction prompt
        }
    }
    
    private void Update()
    {
        if (requireInteraction && playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                TriggerTransition();
            }
        }
    }
    
    private void TriggerTransition()
    {
        if (GameSceneManager.Instance != null && !GameSceneManager.Instance.IsTransitioning())
        {
            GameSceneManager.Instance.LoadScene(targetSceneName, playerSpawnPosition, playerSpawnDirection);
        }
    }
    
    private void OnDrawGizmos()
    {
        // Visualize trigger area
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
        
        // Draw arrow showing spawn direction
        Gizmos.color = Color.blue;
        Vector3 direction = GetDirectionVector();
        Gizmos.DrawRay(transform.position, direction * 2f);
    }
    
    private Vector3 GetDirectionVector()
    {
        return playerSpawnDirection.ToLower() switch
        {
            "up" => Vector3.up,
            "down" => Vector3.down,
            "left" => Vector3.left,
            "right" => Vector3.right,
            "up-left" => new Vector3(-0.7f, 0.7f, 0),
            "up-right" => new Vector3(0.7f, 0.7f, 0),
            "down-left" => new Vector3(-0.7f, -0.7f, 0),
            "down-right" => new Vector3(0.7f, -0.7f, 0),
            _ => Vector3.down
        };
    }
}
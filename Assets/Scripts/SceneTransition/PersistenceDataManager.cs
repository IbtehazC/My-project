using UnityEngine;
using System.Collections.Generic;

public class PersistentDataManager : MonoBehaviour
{
    [Header("Persistent Data")]
    [SerializeField] private bool debugMode = true;
    
    // Data that persists across scenes
    private Dictionary<string, object> persistentGameData = new Dictionary<string, object>();
    
    // Player data
    private Vector3 lastPlayerPosition;
    private string lastPlayerDirection;
    
    public void StoreCurrentSceneData()
    {
        // Store player data
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            lastPlayerPosition = player.transform.position;
            
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                // Store last movement direction
                Vector2 lastDir = movement.LastMoveDirection;
                lastPlayerDirection = DirectionToString(lastDir);
            }
        }
        
        // Store other persistent data here (inventory, quest progress, etc.)
        if (debugMode)
        {
            Debug.Log($"Stored scene data - Player pos: {lastPlayerPosition}, Direction: {lastPlayerDirection}");
        }
    }
    
    public void RestoreSceneData(string sceneName)
    {
        // Restore scene-specific data
        if (debugMode)
        {
            Debug.Log($"Restoring data for scene: {sceneName}");
        }
        
        // Here you would restore things like:
        // - NPC positions and states
        // - Collected items
        // - Quest progress
        // - Time of day
        // etc.
    }
    
    public void StorePersistentData(string key, object data)
    {
        persistentGameData[key] = data;
    }
    
    public T GetPersistentData<T>(string key, T defaultValue = default(T))
    {
        if (persistentGameData.ContainsKey(key))
        {
            try
            {
                return (T)persistentGameData[key];
            }
            catch
            {
                Debug.LogWarning($"Failed to cast persistent data for key: {key}");
                return defaultValue;
            }
        }
        return defaultValue;
    }
    
    public bool HasPersistentData(string key)
    {
        return persistentGameData.ContainsKey(key);
    }
    
    private string DirectionToString(Vector2 direction)
    {
        // Convert direction vector to string
        if (direction.magnitude < 0.1f) return "down";
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360; // Normalize to 0-360
        
        return angle switch
        {
            >= 337.5f or < 22.5f => "right",
            >= 22.5f and < 67.5f => "up-right",
            >= 67.5f and < 112.5f => "up",
            >= 112.5f and < 157.5f => "up-left",
            >= 157.5f and < 202.5f => "left",
            >= 202.5f and < 247.5f => "down-left",
            >= 247.5f and < 292.5f => "down",
            >= 292.5f and < 337.5f => "down-right",
            _ => "down"
        };
    }
}
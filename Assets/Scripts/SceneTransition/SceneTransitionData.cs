using UnityEngine;

[System.Serializable]
public class SceneTransitionData
{
    public string targetScene;
    public Vector3 playerSpawnPosition;
    public string playerSpawnDirection = "down";
    public bool requireInteraction = false;
    
    public SceneTransitionData(string scene, Vector3 spawnPos, string direction = "down")
    {
        targetScene = scene;
        playerSpawnPosition = spawnPos;
        playerSpawnDirection = direction;
    }
}
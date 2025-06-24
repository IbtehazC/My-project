// GardenAreaTrigger.cs - Detects when player enters garden area
using UnityEngine;

public class GardenAreaTrigger : MonoBehaviour
{
    [Header("Garden Area Settings")]
    [SerializeField] private string playerTag = "Player";

    private GardenUI gardenUI;

    private void Start()
    {
        gardenUI = FindObjectOfType<GardenUI>();

        // Ensure trigger is set up
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (gardenUI != null)
            {
                gardenUI.SetInGardenArea(true);
            }

            // Show garden welcome message
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(
                    "Entered Garden Area\nPress G for garden info",
                    NotificationType.Info
                );
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (gardenUI != null)
            {
                gardenUI.SetInGardenArea(false);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize garden area
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
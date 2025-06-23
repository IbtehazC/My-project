using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private LayerMask interactableLayer = -1;
    
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable currentInteractable;
    
    private void Update()
    {
        DetectInteractables();
        HandleInteractionInput();
    }
    
    private void DetectInteractables()
    {
        // Clear previous list
        List<IInteractable> previousInteractables = new List<IInteractable>(nearbyInteractables);
        nearbyInteractables.Clear();
        
        // Find all interactables in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);
        
        foreach (Collider2D col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                nearbyInteractables.Add(interactable);
            }
        }
        
        // Handle enter/exit events for each interactable
        HandleInteractableEvents(previousInteractables, nearbyInteractables);
        
        // Update current interactable (closest one)
        UpdateCurrentInteractable();
    }
    
    private void HandleInteractableEvents(List<IInteractable> previous, List<IInteractable> current)
    {
        // Handle exit events
        foreach (IInteractable interactable in previous)
        {
            if (!current.Contains(interactable))
            {
                interactable.OnInteractionExit();
            }
        }
        
        // Handle enter events
        foreach (IInteractable interactable in current)
        {
            if (!previous.Contains(interactable))
            {
                interactable.OnInteractionEnter();
            }
        }
    }
    
    private void UpdateCurrentInteractable()
    {
        IInteractable newInteractable = null;
        float closestDistance = float.MaxValue;
        
        foreach (IInteractable interactable in nearbyInteractables)
        {
            float distance = Vector2.Distance(transform.position, ((MonoBehaviour)interactable).transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                newInteractable = interactable;
            }
        }
        
        currentInteractable = newInteractable;
    }
    
    private void HandleInteractionInput()
    {
        // Check for interaction with current interactable
        if (currentInteractable != null && currentInteractable.CanInteract())
        {
            KeyCode interactionKey = currentInteractable.GetInteractionKey();
            
            if (Input.GetKeyDown(interactionKey))
            {
                currentInteractable.Interact();
            }
        }
    }
    
    public void TryInteract()
    {
        if (currentInteractable != null && currentInteractable.CanInteract())
        {
            currentInteractable.Interact();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, interactionRange);
    }
}
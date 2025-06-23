using UnityEngine;

public class BasicInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactionPrompt = "Press {KEY} to interact";
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private bool isInteractable = true;

    [Header("Prompt Settings")]
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 2f, 0);
    [SerializeField] private GameObject promptPrefab; // Assign in inspector or use default

    private InteractionPrompt currentPrompt;
    private bool playerInRange = false;

    private void Start()
    {
        // Create prompt if prefab is assigned
        if (promptPrefab != null)
        {
            CreatePrompt();
        }
    }

    private void CreatePrompt()
    {
        // Create prompt instance
        GameObject promptObj = Instantiate(promptPrefab);
        currentPrompt = promptObj.GetComponent<InteractionPrompt>();

        if (currentPrompt != null)
        {
            currentPrompt.Initialize(transform, GetInteractionPrompt(), interactionKey);
            currentPrompt.HidePrompt(); // Start hidden
        }
    }

    public bool CanInteract()
    {
        return isInteractable;
    }

    public void Interact()
    {
        Debug.Log($"Interacted with {gameObject.name}");
        // Add your interaction logic here

        // Example: Pick up item, start dialogue, etc.
        OnInteractionComplete();
    }

    public void OnInteractionEnter()
    {
        playerInRange = true;

        if (currentPrompt != null && CanInteract())
        {
            currentPrompt.ShowPrompt();
            currentPrompt.SetHighlighted(true);
        }

        Debug.Log($"Near {gameObject.name} - {GetInteractionPrompt()}");
    }

    public void OnInteractionExit()
    {
        playerInRange = false;

        if (currentPrompt != null)
        {
            currentPrompt.HidePrompt();
            currentPrompt.SetHighlighted(false);
        }

        Debug.Log($"Left {gameObject.name}");
    }

    public string GetInteractionPrompt()
    {
        return interactionPrompt.Replace("{KEY}", interactionKey.ToString());
    }

    public KeyCode GetInteractionKey()
    {
        return interactionKey;
    }

    public Vector3 GetPromptOffset()
    {
        return promptOffset;
    }

    protected virtual void OnInteractionComplete()
    {
        // Override in derived classes for specific behavior
    }

    private void OnDestroy()
    {
        // Clean up prompt when object is destroyed
        if (currentPrompt != null)
        {
            Destroy(currentPrompt.gameObject);
        }
    }

    // Utility methods
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;

        if (currentPrompt != null)
        {
            if (!interactable && playerInRange)
            {
                currentPrompt.HidePrompt();
            }
            else if (interactable && playerInRange)
            {
                currentPrompt.ShowPrompt();
            }
        }
    }

    public void UpdatePromptText(string newPrompt)
    {
        interactionPrompt = newPrompt;

        if (currentPrompt != null && playerInRange)
        {
            currentPrompt.ShowPrompt(GetInteractionPrompt());
        }
    }
}
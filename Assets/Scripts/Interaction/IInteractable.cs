using UnityEngine;

public interface IInteractable
{
    bool CanInteract();
    void Interact();
    void OnInteractionEnter();
    void OnInteractionExit();
    string GetInteractionPrompt();
    KeyCode GetInteractionKey(); // New: for different interaction keys
    Vector3 GetPromptOffset(); // New: custom prompt positioning
}
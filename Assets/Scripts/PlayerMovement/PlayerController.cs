using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private PlayerMovement movement;
    private PlayerAnimation animations;
    private PlayerInteraction interaction;
    
    [Header("Player State")]
    public PlayerState currentState = PlayerState.Normal;
    
    private void Awake()
    {
        // Get components
        movement = GetComponent<PlayerMovement>();
        animations = GetComponent<PlayerAnimation>();
        interaction = GetComponent<PlayerInteraction>();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // Only process input in normal state
        if (currentState != PlayerState.Normal) return;
        
        // Movement input
        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        
        movement.SetMovementInput(moveInput);
        animations.UpdateMovementAnimation(moveInput, movement.IsMoving);
        
        // Interaction input
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            interaction.TryInteract();
        }
    }
    
    public void SetState(PlayerState newState)
    {
        currentState = newState;
        
        // Handle state changes
        switch (newState)
        {
            case PlayerState.Normal:
                movement.EnableMovement();
                break;
            case PlayerState.Interacting:
            case PlayerState.Cutscene:
                movement.DisableMovement();
                break;
        }
    }
}

public enum PlayerState
{
    Normal,
    Interacting,
    Cutscene
}
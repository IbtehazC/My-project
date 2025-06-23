using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement movement;
    
    // Animation parameter names
    private readonly string SPEED_PARAM = "Speed";
    private readonly string HORIZONTAL_PARAM = "Horizontal";
    private readonly string VERTICAL_PARAM = "Vertical";
    
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
    }
    
    public void UpdateMovementAnimation(Vector2 moveInput, bool isMoving)
    {
        // Set speed for idle/walk transition
        animator.SetFloat(SPEED_PARAM, isMoving ? 1f : 0f);
        
        // For 8-directional movement, we need to normalize input for blend trees
        if (moveInput.magnitude > 0)
        {
            // Normalize the input to ensure proper blend tree positioning
            Vector2 normalizedInput = moveInput.normalized;
            animator.SetFloat(HORIZONTAL_PARAM, normalizedInput.x);
            animator.SetFloat(VERTICAL_PARAM, normalizedInput.y);
        }
        else
        {
            // Keep last direction for idle animation
            Vector2 lastDir = movement.LastMoveDirection;
            animator.SetFloat(HORIZONTAL_PARAM, lastDir.x);
            animator.SetFloat(VERTICAL_PARAM, lastDir.y);
        }
    }
    
    // Debug method to see current blend values
    public void DebugBlendValues()
    {
        Debug.Log($"Speed: {animator.GetFloat(SPEED_PARAM)}, " +
                  $"H: {animator.GetFloat(HORIZONTAL_PARAM)}, " +
                  $"V: {animator.GetFloat(VERTICAL_PARAM)}");
    }
}
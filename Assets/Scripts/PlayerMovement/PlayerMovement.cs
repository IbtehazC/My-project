using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    
    private Vector2 movementInput;
    private Vector2 currentVelocity;
    private Rigidbody2D rb;
    private bool canMove = true;
    
    public bool IsMoving => currentVelocity.magnitude > 0.1f;
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void FixedUpdate()
    {
        if (!canMove)
        {
            // Gradually stop when movement is disabled
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            rb.linearVelocity = currentVelocity;
            return;
        }
        
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        // Calculate target velocity
        Vector2 targetVelocity = movementInput.normalized * moveSpeed;
        
        // Smoothly interpolate to target velocity
        if (movementInput.magnitude > 0)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            LastMoveDirection = movementInput.normalized;
        }
        else
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }
        
        // Apply velocity to rigidbody
        rb.linearVelocity = currentVelocity;
    }
    
    public void SetMovementInput(Vector2 input)
    {
        movementInput = input;
    }
    
    public void EnableMovement()
    {
        canMove = true;
    }
    
    public void DisableMovement()
    {
        canMove = false;
        movementInput = Vector2.zero;
    }
    
    public void StopImmediately()
    {
        currentVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        movementInput = Vector2.zero;
    }
}
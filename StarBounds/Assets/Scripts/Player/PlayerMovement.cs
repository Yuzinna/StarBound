using UnityEngine;
using DG.Tweening; // Make sure to import DOTween

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float moveDuration = 0.1f; // Duration for the move tween

    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float jumpDuration = 0.5f;
    public int numJumps = 1; // For potential double jump in the future

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Interaction")]
    public Transform interactionCheck;
    public float interactionRadius = 0.5f;
    public LayerMask interactableLayer;

    [HideInInspector]
    public bool isGrounded;

    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private bool isInteracting = false;
    private FixedJoint2D currentJoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody2D component on the GameObject.");
            enabled = false; // Disable script if Rigidbody2D is missing
        }
    }

    void Update()
    {
        // Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isInteracting)
        {
            // Input handling for movement
            float moveInput = Input.GetAxis("Horizontal");
            Move(moveInput);
        }

        // Jump Input
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded && !isInteracting)
        {
            Jump();
        }

        // Interaction Input
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        // If already interacting (e.g., holding a cube), this input stops the interaction.
        if (isInteracting)
        {
            isInteracting = false;
            // If a joint was created for the interaction, destroy it.
            if (currentJoint != null)
            {
                Destroy(currentJoint);
            }
            return;
        }

        // Detect all interactable objects within the interaction radius.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(interactionCheck.position, interactionRadius, interactableLayer);
        foreach (var collider in colliders)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                // Call the Interact method on the found object (e.g., GravitySwitch, Cube).
                interactable.Interact();

                // Special case for the Cube: If it's movable, start the push/pull state.
                Cube cube = collider.GetComponent<Cube>();
                if (cube != null && cube.canBeMoved)
                {
                    Rigidbody2D cubeRb = cube.GetComponent<Rigidbody2D>();
                    if (cubeRb != null) // Ensure the cube has a Rigidbody2D
                    {
                        isInteracting = true;
                        // Create a joint to connect the player and the cube, allowing push/pull.
                        currentJoint = gameObject.AddComponent<FixedJoint2D>();
                        currentJoint.connectedBody = cubeRb;
                        currentJoint.enableCollision = false; // Prevent weird physics collisions between player and cube.
                    }
                    else
                    {
                        Debug.LogWarning("Cube object is interactable but missing Rigidbody2D for joint interaction.");
                    }
                }
                
                // Only interact with the first object found to prevent multiple interactions at once.
                break; 
            }
        }
    }

    public void Move(float direction)
    {
        // Use Rigidbody2D.velocity for continuous horizontal movement for physics consistency.
        // DOTween can be used for visual effects if desired, but not for core physics movement.
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // Flip character sprite if necessary
        if (direction > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction < 0 && isFacingRight)
        {
            Flip();
        }
    }

    public void Jump()
    {
        if (isGrounded)
        {
            // Use DOTween for jump animation/tweening.
            // DOJump takes jumpPower (height) and duration.
            transform.DOJump(transform.position, jumpForce, numJumps, jumpDuration).SetEase(Ease.OutQuad);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // Visualize interaction radius in editor
    private void OnDrawGizmosSelected()
    {
        if (interactionCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionCheck.position, interactionRadius);
        }
    }
}
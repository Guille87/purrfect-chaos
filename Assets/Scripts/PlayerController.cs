using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    Vector2 moveInput;
    PlayerInput playerInput;

    public float speed = 5f;
    bool isClimbing = false;
    bool isDead = false;
    bool isTouchingRope = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
    }

    void OnDisable()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (isDead) return;

        moveInput.Normalize();

        // Apply movement only if player is not climbing
        if (!isClimbing)
        {
            rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
            animator.SetBool("isRunning", moveInput.x != 0);

            // Flip the sprite based on the direction of movement
            if (moveInput.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (moveInput.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        //if (isTouchingRope)
        //{
            if (moveInput.y != 0)
            {
                isClimbing = true;
                animator.SetBool("isClimbing", true);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveInput.y * speed);
                rb.gravityScale = 0;
            }
            else
            {
                isClimbing = false;
                animator.SetBool("isClimbing", false);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }
        /*}
        else
        {
            //rb.gravityScale = 1;
        }*/
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (context.performed)
        {
            isTouchingRope = true;
        }
        else if (context.canceled)
        {
            isTouchingRope = false;
        }
    }

    public void OnAttack()
    {
        if (!isDead) animator.SetTrigger("isAttacking");
    }

    public void OnDeath()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
    }
}

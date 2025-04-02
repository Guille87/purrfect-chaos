using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    Vector2 moveInput;
    PlayerInput playerInput;

    public float speed = 3f;
    bool isClimbing = false;
    bool isDead = false;
    bool isTouchingRope = false;
    bool isGrounded = false;

    public float groundCheckDistance = 0.03f;
    public LayerMask groundLayer;

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
        playerInput.actions["Attack"].performed += OnAttack;
    }

    void OnDisable()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
        playerInput.actions["Attack"].performed -= OnAttack;
    }

    void FixedUpdate()
    {
        isGrounded = IsGrounded();

        // Revisar constantemente si está tocando la cuerda
        isTouchingRope = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Rope"));

        if (isTouchingRope && !isGrounded && !isClimbing)
        {
            StartClimbing();
        }

        Move();

        if (!isGrounded && !isClimbing)
        {
            ApplyGridSnap();
        }
    }

    void StartClimbing()
    {
        isClimbing = true;
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        AlignToNearestRope();
        animator.SetBool("isClimbing", true);
    }

    void Move()
    {
        if (isDead) return;

        // Actualizar si aún está tocando la cuerda
        isTouchingRope = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Rope"));
        
        // Movimiento en la cuerda
        if (isClimbing)
        {
            rb.gravityScale = 0; // Desactivar gravedad

            if (moveInput.y != 0)
            {
                rb.linearVelocity = new Vector2(0, moveInput.y * speed);
                animator.SetBool("isClimbing", true);
                animator.SetBool("isRunning", false);
                animator.SetBool("isClimbingIdle", false);
                AlignToNearestRope();
            }
            else if (moveInput.x != 0)
            {
                rb.linearVelocity = new Vector2(moveInput.x * speed, 0);
                animator.SetBool("isRunning", true);
                animator.SetBool("isClimbing", false);
                animator.SetBool("isClimbingIdle", false);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isClimbingIdle", true);
                animator.SetBool("isClimbing", false);
                animator.SetBool("isRunning", false);
            }

            // Si ya no está tocando la cuerda, salir de escalada
            if (!isTouchingRope)
            {
                isClimbing = false;
                rb.gravityScale = 1;
                Debug.Log("Salió de la cuerda");
            }
        }
        // Si está tocando la cuerda pero aún no está escalando, permitirlo
        else if (isTouchingRope && moveInput.y != 0)
        {
            isClimbing = true;
            animator.SetBool("isClimbing", true);
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(0, moveInput.y * speed);
            // Aplicar alineación a la cuerda más cercana
            AlignToNearestRope();
        }
        // Movimiento normal en el suelo
        else if (isGrounded)
        {
            rb.gravityScale = 1;
            animator.SetBool("isClimbing", false);
            animator.SetBool("isClimbingIdle", false);

            if (moveInput.x != 0)
            {
                rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
                animator.SetBool("isRunning", true);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isRunning", false);
            }
        }
        // Movimiento en el aire
        else
        {
            rb.gravityScale = 1;

            // Prohibir movimiento horizontal en el aire
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            animator.SetBool("isRunning", false);
        }

        // Girar sprite según dirección
        if (moveInput.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void AlignToNearestRope()
    {
        float gridSize = 1f; // Tamaño de la cuadrícula

        // Buscar la cuerda más cercana dentro de un pequeño rango
        Collider2D nearestRope = Physics2D.OverlapCircle(transform.position, 1f, LayerMask.GetMask("Rope"));

        if (nearestRope != null)
        {
            // Ajustar la posición X al centro de la cuerda
            transform.position = new Vector3(
                Mathf.Round(nearestRope.transform.position.x / gridSize) * gridSize, 
                transform.position.y, 
                transform.position.z
            );
        }
    }

    void ApplyGridSnap()
    {
        float gridSize = 1f; // Tamaño de la cuadrícula

        // Redondea la posición X a los valores de la cuadrícula
        transform.position = new Vector3(
            Mathf.Round(transform.position.x / gridSize) * gridSize,
            transform.position.y,
            transform.position.z
        );
    }

    bool IsGrounded()
    {
        Collider2D col = GetComponent<Collider2D>(); // Obtener el Collider del jugador
        Bounds bounds = col.bounds; // Obtener los límites del collider

        // Posiciones de los raycasts en los bordes inferiores del collider
        Vector2 leftOrigin = new Vector2(bounds.min.x, bounds.min.y);
        Vector2 rightOrigin = new Vector2(bounds.max.x, bounds.min.y);

        RaycastHit2D leftHit = Physics2D.Raycast(leftOrigin, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightOrigin, Vector2.down, groundCheckDistance, groundLayer);

        Debug.DrawRay(leftOrigin, Vector2.down * groundCheckDistance, leftHit.collider ? Color.red : Color.green); // Visualizar Raycast izquierdo
        Debug.DrawRay(rightOrigin, Vector2.down * groundCheckDistance, rightHit.collider ? Color.red : Color.blue); // Visualizar Raycast derecho

        return leftHit.collider != null || rightHit.collider != null; // Es true si al menos uno detecta suelo
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        if (!isDead)
        {
            animator.SetTrigger("isAttacking");
        }
    }

    public void OnDeath()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Rope"))
        {
            isTouchingRope = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Rope"))
        {
            isTouchingRope = false;
        }
    }
}

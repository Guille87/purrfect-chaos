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

    public float groundCheckDistance = 0.2f;
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
        Move();

        if (!isGrounded)
        {
            ApplyGridSnap();
        }
    }

    void Move()
    {
        if (isDead) return;

        // Si está en la cuerda, poder moverse verticalmente
        if (isClimbing)
        {
            rb.gravityScale = 0; // Desactivar gravedad al escalar
            
            if (moveInput.y != 0)
            {
                rb.linearVelocity = new Vector2(0, moveInput.y * speed);
                Debug.Log("Escalando: Movimiento vertical");
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0; // Mantener gravedad desactivada al estar quieto en la cuerda
                Debug.Log("Escalando: Quieto en la cuerda");
            }

            // Activar animación de escalar
            animator.SetBool("isClimbing", true);
        }
        else
        {
            animator.SetBool("isClimbing", false);
            rb.gravityScale = 1;

            // Movimiento horizontal solo si está en el suelo
            if (isGrounded)
            {
                float horizontalInput = moveInput.x;

                if (horizontalInput != 0)
                {
                    // Mover de 0.1 en 0.1 unidades
                    float targetX = Mathf.Round(transform.position.x / 0.1f) * 0.1f + horizontalInput * 0.1f;
                    transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
                    animator.SetBool("isRunning", true);
                }
                else
                {
                    animator.SetBool("isRunning", false);
                }
            }
            else
            {
                // Si está en el aire, no permitir movimiento horizontal
                rb.gravityScale = 1;
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                // Detener la animación de correr si no se mueve horizontalmente
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
            
            if (rb.linearVelocity.y < 0)
            {
                rb.gravityScale = 1;
            }
        }

        // Si está tocando la cuerda y se mueve verticalmente, escalar
        if (isTouchingRope)
        {
            if (moveInput.y != 0)
            {
                isClimbing = true;
                animator.SetBool("isClimbing", true);
                rb.gravityScale = 0;
                rb.linearVelocity = new Vector2(0, moveInput.y * speed);
            }
            else
            {
                isClimbing = false;
                animator.SetBool("isClimbing", false);
                //rb.linearVelocity = new Vector2(moveInput.x * speed, 0);
            }
        }
        else
        {
            // No está tocando la cuerda, hacer que el personaje vuelva al suelo o al aire
            Debug.Log("No está tocando la cuerda, no se puede escalar.");
            isClimbing = false;
            animator.SetBool("isClimbing", false);
            //rb.gravityScale = 1; // Activar gravedad al salir de la cuerda
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

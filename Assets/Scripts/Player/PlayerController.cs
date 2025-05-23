using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    Vector2 moveInput;
    PlayerInput playerInput;

    public float speed = 3f;
    bool isAttacking = false;
    bool isClimbing = false;
    bool isDead = false;
    bool isTouchingRope = false;
    bool isGrounded = false;

    public float groundCheckDistance = 0.03f;
    public LayerMask groundLayer;
    public Vector2 attackBoxSize = new Vector2(0.4f, 0.6f);
    public LayerMask pillarLayer;
    public LayerMask breakableLayer;
    public bool IsDead => isDead;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip footstepsSound;
    public AudioClip ropeSlideSound;
    public AudioClip fallingSound;
    
    private bool isPlayingFootsteps = false;
    private bool isPlayingropeSlideSound = false;
    private bool isFalling = false;
    private float fallDuration = 0f;
    bool hasBeenHit = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        playerInput.enabled = false; // Desactivar el PlayerInput al inicio
        StartCoroutine(WaitForIntro());
    }

    private IEnumerator WaitForIntro()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;
        playerInput.enabled = true; // Activar el PlayerInput después de la intro
    }

    void OnEnable()
    {
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Attack"].performed += OnAttack;
        playerInput.actions["Pause"].performed += OnPause;
    }

    void OnDisable()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
        playerInput.actions["Attack"].performed -= OnAttack;
        playerInput.actions["Pause"].performed -= OnPause;
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking) return;
        
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
        // Movimiento en la cuerda
        if (isClimbing)
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerClimbing");
            rb.gravityScale = 0; // Desactivar gravedad

            if (moveInput.y != 0)
            {
                if (moveInput.y < 0)
                {
                    Vector2 abajo = new Vector2(transform.position.x, transform.position.y - 0.7f);
                    Collider2D cuerdaDebajo = Physics2D.OverlapCircle(abajo, 0.2f, LayerMask.GetMask("Rope"));

                    if (cuerdaDebajo == null)
                    {
                        // No hay cuerda debajo, no permitimos bajar más
                        rb.linearVelocity = Vector2.zero;
                        animator.SetBool("isClimbingIdle", true);
                        animator.SetBool("isClimbing", false);
                        animator.SetBool("isRunning", false);
                        return;
                    }
                }

                // Reproducir sonido de deslizamiento por la cuerda si no está ya reproduciéndose
                StartRopeSlideSound();
                
                gameObject.layer = LayerMask.NameToLayer("PlayerClimbing");
                rb.linearVelocity = new Vector2(0, moveInput.y * speed);
                animator.SetBool("isClimbing", true);
                animator.SetBool("isRunning", false);
                animator.SetBool("isClimbingIdle", false);
                AlignToNearestRope();
            }
            else if (moveInput.x != 0)
            {
                // Reproducir sonido de deslizamiento por la cuerda si no está ya reproduciéndose
                StartRopeSlideSound();
                
                gameObject.layer = LayerMask.NameToLayer("Player");
                rb.linearVelocity = new Vector2(moveInput.x * speed, 0);
                animator.SetBool("isRunning", true);
                animator.SetBool("isClimbing", false);
                animator.SetBool("isClimbingIdle", false);
            }
            else
            {
                // Detener el sonido de deslizamiento por la cuerda si el jugador no se está moviendo
                StopRopeSlideSound();

                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isClimbingIdle", true);
                animator.SetBool("isClimbing", false);
                animator.SetBool("isRunning", false);
            }

            // Si ya no está tocando la cuerda, salir de escalada
            if (!isTouchingRope)
            {
                isClimbing = false;
                gameObject.layer = LayerMask.NameToLayer("Player");

                // Detenemos completamente la velocidad vertical para evitar "saltitos"
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

                rb.gravityScale = 1;
            }
            StopFootsteps(); // Detener el sonido de pasos si está escalando
            StopFallingSound(); // Detener el sonido de caída si está escalando
        }
        // Si está tocando la cuerda pero aún no está escalando, permitirlo
        else if (isTouchingRope && moveInput.y != 0)
        {
            isClimbing = true;
            gameObject.layer = LayerMask.NameToLayer("PlayerClimbing");
            animator.SetBool("isClimbing", true);
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(0, moveInput.y * speed);
            // Aplicar alineación a la cuerda más cercana
            AlignToNearestRope();
        }
        // Movimiento normal en el suelo
        else if (isGrounded)
        {
            // Si está en el suelo y pulsa abajo, y hay una cuerda justo debajo, atravesar la plataforma
            if (moveInput.y < 0)
            {
                Vector2 abajo = new Vector2(transform.position.x, transform.position.y - 0.7f);
                Collider2D cuerdaDebajo = Physics2D.OverlapCircle(abajo, 0.2f, LayerMask.GetMask("Rope"));
                if (cuerdaDebajo != null)
                {
                    // Bajamos al jugador para salir del suelo
                    transform.position = new Vector3(transform.position.x, transform.position.y - 0.4f, transform.position.z);
                    // Activamos el modo de escalada
                    isClimbing = true;
                    gameObject.layer = LayerMask.NameToLayer("PlayerClimbing");
                    rb.gravityScale = 0;
                    AlignToNearestRope();
                    return; // Salimos de este bloque para que no se procese el resto como si estuviera en el suelo
                }
            }

            rb.gravityScale = 1;
            animator.SetBool("isClimbing", false);
            animator.SetBool("isClimbingIdle", false);

            if (moveInput.x != 0)
            {
                rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
                animator.SetBool("isRunning", true);

                // Reproducir sonido de pasos si no está ya reproduciéndose
                StartFootsteps();
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isRunning", false);

                // Detener el sonido de los pasos si el jugador no se está moviendo
                StopFootsteps();
            }
            StopRopeSlideSound(); // Detener el sonido de deslizamiento por la cuerda si el jugador está en el suelo
            StopFallingSound(); // Detener el sonido de caída si el jugador está en el suelo
        }
        // Movimiento en el aire
        else
        {
            rb.gravityScale = 1;

            // Prohibir movimiento horizontal en el aire
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            animator.SetBool("isRunning", false);

            if (rb.linearVelocity.y < 0)
            {
                StartFallingSound();
            }
            
            if (isFalling)
            {
                fallDuration += Time.deltaTime;
            }

            // Detener el sonido de los pasos si el jugador está en el aire
            StopFootsteps();
            StopRopeSlideSound();
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
        float searchRadius = 1f;
        float direction = Mathf.Sign(moveInput.x != 0 ? moveInput.x : transform.localScale.x);

        // Buscar todas las cuerdas cercanas
        Collider2D[] ropes = Physics2D.OverlapCircleAll(transform.position, searchRadius, LayerMask.GetMask("Rope"));

        if (ropes.Length > 0)
        {
            Collider2D bestRope = null;
            float closestDistance = float.MaxValue;

            foreach (var rope in ropes)
            {
                float deltaX = rope.transform.position.x - transform.position.x;

                // Si estamos moviéndonos horizontalmente, solo considerar cuerdas en esa dirección
                if (moveInput.x != 0 && Mathf.Sign(deltaX) != direction)
                    continue;

                float distance = Mathf.Abs(deltaX);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestRope = rope;
                }
            }

            // Si encontramos una cuerda válida, alinearse a ella
            if (bestRope != null)
            {
                transform.position = new Vector3(
                    Mathf.Round(bestRope.transform.position.x / gridSize) * gridSize,
                    transform.position.y,
                    transform.position.z
                );
            }
        }
    }

    private void StartFootsteps()
    {
        if (audioSource != null && footstepsSound != null && !audioSource.isPlaying && !isPlayingFootsteps)
        {
            audioSource.clip = footstepsSound;
            audioSource.loop = true;
            audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
            audioSource.Play();
            isPlayingFootsteps = true;
        }
    }

    private void StopFootsteps()
    {
        if (audioSource != null && audioSource.isPlaying && isPlayingFootsteps)
        {
            audioSource.Stop();
            isPlayingFootsteps = false;
        }
    }

    private void StartRopeSlideSound()
    {
        if (audioSource != null && footstepsSound != null && !audioSource.isPlaying && !isPlayingropeSlideSound)
        {
            audioSource.clip = ropeSlideSound;
            audioSource.loop = true;
            audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
            audioSource.Play();
            isPlayingropeSlideSound = true;
        }
    }

    private void StopRopeSlideSound()
    {
        if (audioSource != null && audioSource.isPlaying && isPlayingropeSlideSound)
        {
            audioSource.Stop();
            isPlayingropeSlideSound = false;
        }
    }

    private void StartFallingSound()
    {
        if (audioSource != null && fallingSound != null && !audioSource.isPlaying && !isFalling)
        {
            audioSource.clip = fallingSound;
            audioSource.loop = false;
            audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
            audioSource.Play();
            isFalling = true;
            fallDuration = 0f;  // Reiniciar el contador de la duración de la caída
        }
    }

    private void StopFallingSound()
    {
        if (audioSource != null && audioSource.isPlaying && isFalling)
        {
            audioSource.Stop();
            isFalling = false;
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

        Vector2 lockedDirection = Vector2.zero;

        if (context.performed)
        {
            if (lockedDirection == Vector2.zero && moveInput != Vector2.zero)
            {
                // Bloqueamos solo una dirección, vertical o horizontal
                if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                    lockedDirection = new Vector2(Mathf.Sign(moveInput.x), 0); // Solo izquierda/derecha
                else
                    lockedDirection = new Vector2(0, Mathf.Sign(moveInput.y)); // Solo arriba/abajo
            }
        }
        else if (context.canceled)
        {
            lockedDirection = Vector2.zero; // Se libera el control
        }

        moveInput = lockedDirection;
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        if (!isDead && !isAttacking && !isClimbing && isGrounded)
        {
            StopFootsteps(); // Detener el sonido de los pasos al atacar
            
            isAttacking = true;
            rb.linearVelocity = Vector2.zero;
            animator.SetTrigger("isAttacking");
        }
    }

    public void PerformAttack()
    {
        // Golpea los pilares
        Collider2D[] hitPilares = Physics2D.OverlapBoxAll(transform.position, attackBoxSize, 0f, pillarLayer);
        // Golpea los vasos
        Collider2D[] hitVasos = Physics2D.OverlapBoxAll(transform.position, attackBoxSize, 0f, breakableLayer);

        if (hitPilares.Length > 0 || hitVasos.Length > 0)
        {
            // Combina los dos arrays de objetos detectados (vasos y pilares)
            var allHits = hitPilares.Cast<Collider2D>().Concat(hitVasos).ToArray();

            // Ordenar todos los objetos detectados por la distancia al jugador
            Collider2D closestObject = allHits.OrderBy(o => Vector2.Distance(transform.position, o.transform.position)).First();

            // Verificar si es un pilar o un vaso y ejecutar la acción correspondiente
            PilarController pilar = closestObject.GetComponent<PilarController>();
            if (pilar != null)
            {
                pilar.Romper();
                return; // Si se golpeó un pilar, ya no buscamos más.
            }

            VasoController vaso = closestObject.GetComponent<VasoController>();
            if (vaso != null)
            {
                vaso.Golpear();
            }
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void OnDeath()
    {
        StopFootsteps(); // Detener el sonido de los pasos al morir
        StopRopeSlideSound(); // Detener el sonido de deslizamiento por la cuerda al morir
        StopFallingSound(); // Detener el sonido de caída al morir
        
        isDead = true;
        gameObject.layer = LayerMask.NameToLayer("Dead");
        // Desactivar colisiones
        foreach (var col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }
        playerInput.enabled = false; // Desactivar el PlayerInput
        animator.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1; // Activar gravedad al morir

        // Restar vida en el GameManager
        GameManager.Instance.PerderVida();
        
        StartCoroutine(DeathSequence());
    }

    void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed && !isDead)
        {
            StopFootsteps(); // Detener el sonido de los pasos al pausar
            StopRopeSlideSound(); // Detener el sonido de deslizamiento por la cuerda al pausar
            
            GameManager.Instance.TogglePause();
        }
    }

    private IEnumerator DeathSequence()
    {
        // Hacer que el jugador salte un poco hacia arriba
        rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        
        // Deshabilitar las colisiones con otros objetos
        Collider2D col = GetComponent<Collider2D>();
        col.enabled = false;

        // Esperar 4 segundos
        yield return new WaitForSeconds(4f);

        if (GameManager.Instance.Vidas <= 0)
        {
            // Si el jugador no tiene vidas, mostrar Game Over
            GameManager.Instance.GameOver();
        }
        else
        {
            // Si el jugador tiene vidas, reiniciar el juego
            GameManager.Instance.ReiniciarJuego();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasBeenHit && collision.CompareTag("Enemy"))
        {
            hasBeenHit = true;
            // Llamamos a OnDeath solo si el jugador no está muerto
            if (!isDead)
            {
                OnDeath();
            }
        }

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

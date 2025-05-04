using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 2f;
    public bool flipSprite = true;

    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float moveDirection = 1f; // 1 para derecha, -1 para izquierda
    private bool isClimbing = false;
    private bool wasInAir = false;
    private int ropeTouchCount = 0;
    private string currentAnimation = "";
    
    private int defaultLayer;
    private int ignorePlatformLayer;

    private Vector2 lastPosition;
    private float stuckTime = 0f;
    private float stuckCheckInterval = 0.25f;
    private float stuckThreshold = 0.01f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        defaultLayer = gameObject.layer;
        ignorePlatformLayer = LayerMask.NameToLayer("EnemyIgnorePlatform");

        lastPosition = transform.position;
    }

    void Update()
    {
        // Si no hay referencia al jugador, salir de la función
        if (player == null) return;

        // Calcular la distancia vertical entre el enemigo y el jugador
        float verticalDistance = player.position.y - transform.position.y;

        // Buscar si hay una cuerda alineada con el enemigo
        Transform alignedRope = GetAlignedRope();

        // Si hay una cuerda alineada y el jugador está a una distancia vertical considerable
        if (alignedRope != null && Mathf.Abs(verticalDistance) > 0.1f)
        {
            isClimbing = true;

            // Si el jugador está por encima o si la cuerda se puede trepar (por no estar bloqueada por una plataforma)
            if (verticalDistance > 0f || CanClimbRope(alignedRope))
            {
                gameObject.layer = ignorePlatformLayer;
                Debug.DrawLine(transform.position, transform.position + Vector3.down * 0.7f, Color.green);
                
                // Determinar la dirección horizontal hacia el jugador
                moveDirection = Mathf.Sign(player.position.x - transform.position.x);

                // Trepar la cuerda en dirección al jugador
                Climb(verticalDistance);
            }
            else
            {
                // Si hay una plataforma debajo, cambiar el layer a defaultLayer y mover normalmente
                if (Physics2D.Raycast(transform.position, Vector2.down, 0.7f, LayerMask.GetMask("Plataforma")))
                {
                    gameObject.layer = defaultLayer;
                    NormalMovement();
                }
            }
        }
        // Si hay cuerda alineada, estamos alineados con el jugador y hay un camino libre hacia él
        else if (alignedRope != null && IsAlignedWithPlayer() && HasClearPathToPlayer())
        {
            TryFlipTowardsPlayer();
            MoveHorizontallyOnRope();
        }
        else
        {
            // Si no hay cuerda alineada, mover normalmente
            NormalMovement();
        }

        // Si el enemigo no está trepando y el jugador está más abajo, intentar bajar a una cuerda inferior
        if (!isClimbing && player.position.y < transform.position.y)
        {
            TryDownToRopeBelow();
        }
    }

    void TryFlipTowardsPlayer()
    {
        if (Mathf.Sign(player.position.x - transform.position.x) != Mathf.Sign(moveDirection))
        {
            moveDirection *= -1;
            if (flipSprite)
                spriteRenderer.flipX = moveDirection < 0;
        }
    }

    void MoveHorizontallyOnRope()
    {
        rb.gravityScale = ropeTouchCount > 0 ? 0f : 1f;
        rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y * rb.gravityScale);

        gameObject.layer = defaultLayer;
        UpdateAnimation();
        if (flipSprite)
            spriteRenderer.flipX = moveDirection < 0;
    }

    void NormalMovement()
    {
        isClimbing = false;
        rb.gravityScale = 1f;
        rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);
        gameObject.layer = defaultLayer;

        UpdateAnimation();
        if (flipSprite)
            spriteRenderer.flipX = moveDirection < 0;
        
        stuckTime += Time.deltaTime;
        if (stuckTime >= stuckCheckInterval)
        {
            float distanceMoved = Mathf.Abs(transform.position.x - lastPosition.x);
            if (distanceMoved < stuckThreshold)
            {
                // Cambiar dirección si casi no se movió
                moveDirection *= -1;
                if (flipSprite)
                    spriteRenderer.flipX = moveDirection < 0;
            }
            lastPosition = transform.position;
            stuckTime = 0f;
        }
    }

    void TryDownToRopeBelow()
    {
        Vector2 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.7f, LayerMask.GetMask("Rope"));

        if (hit.collider && Mathf.Abs(hit.collider.transform.position.x - transform.position.x) < 0.05f)
        {
            isClimbing = true;
            gameObject.layer = ignorePlatformLayer;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0f, -speed);
            UpdateAnimation();
        }
    }

    void Climb(float verticalDistance)
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(0f, verticalDistance > 0f ? speed : -speed);
        ChangeAnimation("EnemigoPerro1Climb");
    }

    void RecalculateDirection()
    {
        if (player == null) return;
        moveDirection = Mathf.Sign(player.position.x - transform.position.x);
        if (flipSprite)
            spriteRenderer.flipX = moveDirection < 0;
    }

    bool CanClimbRope(Transform rope)
    {
        RaycastHit2D hit = Physics2D.Raycast(rope.position, Vector2.down, 2f, LayerMask.GetMask("Plataforma", "Container"));

        // Si detectamos una plataforma o contenedor, el descenso está bloqueado
        return hit.collider == null;
    }

    bool IsAlignedWithPlayer()
    {
        return Mathf.Abs(transform.position.y - player.position.y) < 0.01f;
    }

    bool HasClearPathToPlayer()
    {
        Vector2 direction = new Vector2(player.position.x - transform.position.x, 0).normalized;
        float distance = Mathf.Abs(player.position.x - transform.position.x);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, LayerMask.GetMask("Plataforma", "Container"));
        return hit.collider == null;
    }

    Transform GetAlignedRope()
    {
        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, 0.2f, LayerMask.GetMask("Rope")))
        {
            if (Mathf.Abs(hit.transform.position.x - transform.position.x) < 0.05f)
                return hit.transform;
        }
        return null;
    }

    void UpdateAnimation()
    {
        if (isClimbing)
        {
            ChangeAnimation("EnemigoPerro1Climb");
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            ChangeAnimation("EnemigoPerro1Run");
        }
        else
        {
            ChangeAnimation("EnemigoPerro1Idle");
        }
    }

    void ChangeAnimation(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;
        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Rope"))
        {
            ropeTouchCount++;
            rb.gravityScale = 0f;
        }

        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !playerController.IsDead)
            {
                playerController.OnDeath();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Rope"))
        {
            ropeTouchCount = Mathf.Max(0, ropeTouchCount - 1);
            if (ropeTouchCount == 0)
            {
                rb.gravityScale = 1f;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Plataforma"))
        {
            // Iteramos sobre los puntos de contacto de la colisión
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Verificamos si el contacto ocurre en los laterales de la plataforma
                if (Mathf.Abs(contact.normal.x) > Mathf.Abs(contact.normal.y))
                {
                    // Comprobamos si la distancia horizontal es pequeña, para ver si está cerca de los bordes
                    float horizontalDistance = Mathf.Abs(contact.point.x - transform.position.x);

                    // Si la distancia horizontal es pequeña, significa que el enemigo está cerca del borde de la plataforma
                    if (horizontalDistance < 0.5f)
                    {
                        // Verificamos la diferencia de altura entre el enemigo y la plataforma
                        float heightDifference = Mathf.Abs(transform.position.y - collision.transform.position.y);

                        // Si la diferencia de altura es pequeña, ajustamos la posición del enemigo
                        if (heightDifference < 0.5f) // Margen de corrección para la altura
                        {
                            float platformHeight = collision.transform.position.y + collision.collider.bounds.extents.y;
                            transform.position = new Vector2(transform.position.x, platformHeight);
                        }
                    }
                }
            }

            if (wasInAir)
            {
                RecalculateDirection();
                wasInAir = false;
            }
        }
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Container"))
        {
            // Iteramos sobre los puntos de contacto de la colisión
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Verificamos si el punto de contacto está en los lados del contenedor
                if (Mathf.Abs(contact.normal.x) > Mathf.Abs(contact.normal.y))
                {
                    // Si el contacto ocurre por un lado (normal.x es más fuerte que normal.y), cambiamos la dirección
                    moveDirection *= -1;

                    if (flipSprite)
                        spriteRenderer.flipX = moveDirection < 0;

                    // Salimos del bucle porque ya hemos cambiado la dirección
                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Plataforma"))
        {
            wasInAir = true;
        }
    }
}

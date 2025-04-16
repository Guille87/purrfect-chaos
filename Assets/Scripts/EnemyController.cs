using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 2f;
    public bool flipSprite = true;

    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        // Movimiento horizontal solo si no está en escalada
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        UpdateAnimation(direction);

        if (flipSprite)
        {
            if (direction.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (direction.x < 0)
            {
                spriteRenderer.flipX = true;
            }
        }
    }

    private void UpdateAnimation(Vector2 direction)
    {
        // Cambia a Run solo si hay movimiento horizontal suficiente
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("EnemigoPerro1Run"))
                animator.Play("EnemigoPerro1Run");
        }
        else
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("EnemigoPerro1Idle"))
                animator.Play("EnemigoPerro1Idle");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Jugador tocado! Pierde una vida.");
        }
    }
}

using UnityEngine;

public class ContenedorController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    bool isFalling = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Drop()
    {
        if (!isFalling)
        {
            isFalling = true;
            animator.SetBool("isFalling", true);
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFalling && collision.gameObject.CompareTag("Plataforma"))
        {
            isFalling = false;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            transform.rotation = Quaternion.Euler(0, 0, 100);
        }
    }
}

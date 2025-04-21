using UnityEngine;

public class VasoController : MonoBehaviour
{
    public enum TipoVaso { Vaso1, Vaso2, Vaso3 }
    public TipoVaso tipoVaso;

    private SpriteRenderer sr;
    private Animator animator;
    private int golpes = 0;

    public Sprite grieta1;
    public Sprite grieta2;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        animator.enabled = false; // Desactivamos el animator al inicio
    }

    public void Golpear()
    {
        golpes++;

        switch (tipoVaso)
        {
            case TipoVaso.Vaso1:
                Romper();
                GameManager.Instance.SumarPuntos(100);
                break;

            case TipoVaso.Vaso2:
                if (golpes == 1)
                {
                    sr.sprite = grieta1;
                }
                else
                {
                    Romper();
                    GameManager.Instance.SumarPuntos(200);
                }
                break;

            case TipoVaso.Vaso3:
                if (golpes == 1)
                {
                    sr.sprite = grieta1;
                }
                else if (golpes == 2)
                {
                    sr.sprite = grieta2;
                }
                else
                {
                    Romper();
                    GameManager.Instance.SumarPuntos(300);
                }
                break;
        }
    }

    private void Romper()
    {
        if (animator != null)
        {
            animator.enabled = true; // Activamos el animator para la animación de romper
            animator.SetTrigger("Romper");
            Destroy(gameObject, 0.3f);
        }
    }
}

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

    public AudioClip sfxGolpe;
    public AudioClip sfxRomper;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (sr == null)
        Debug.LogError("Falta el SpriteRenderer en " + name);

        if (animator == null)
            Debug.LogError("Falta el Animator en " + name);
        
        animator.enabled = false; // Desactivamos el animator al inicio
    }

    public void Golpear()
    {
        golpes++;

        switch (tipoVaso)
        {
            case TipoVaso.Vaso1:
                Romper(100);
                break;

            case TipoVaso.Vaso2:
                if (golpes == 1)
                {
                    sr.sprite = grieta1;
                    ReproducirGolpe();
                }
                else
                {
                    Romper(200);
                }
                break;

            case TipoVaso.Vaso3:
                if (golpes == 1)
                {
                    sr.sprite = grieta1;
                    ReproducirGolpe();
                }
                else if (golpes == 2)
                {
                    sr.sprite = grieta2;
                    ReproducirGolpe();
                }
                else
                {
                    Romper(300);
                }
                break;
        }
    }

    private void ReproducirGolpe()
    {
        // Reproducir el sonido de golpe
        if (sfxGolpe != null)
            AudioManager.PlaySound(sfxGolpe);
    }

    private void Romper(int puntos)
    {
        if (sfxRomper != null)
            AudioManager.PlaySound(sfxRomper);
        
        if (animator != null)
        {
            animator.enabled = true; // Activamos el animator para la animación de romper
            animator.SetTrigger("Romper");
        }

        GameManager.Instance.SumarPuntos(puntos);
        GameManager.Instance.VasoRoto(this);
        Destroy(gameObject, 0.3f);
    }
}

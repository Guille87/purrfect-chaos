using System.Collections;
using UnityEngine;

public class ContenedorController : MonoBehaviour
{
    private Animator animator;
    private float tiempoCaida = 0.25f;  // Tiempo total para la caída
    private float distanciaCaida = 1f;  // Distancia que se mueve en cada caída

    public bool SeEstaCayendo { get; private set; } = false;

    [Header("Audio")]
    public AudioClip fallingSound;
    public AudioClip loseLifeSound;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Romper()
    {
        animator.SetTrigger("Romper");
        StartCoroutine(Caer());
    }

    private IEnumerator Caer()
    {
        yield return new WaitForSeconds(0.5f);  // Espera un poco antes de iniciar la caída

        if (fallingSound != null)
        {
            AudioManager.PlaySoundWithPitch(fallingSound, 0.5f, 2.25f); // Reproducir sonido de caída con un rango de pitch
        }

        SeEstaCayendo = true;  // Marcamos que el contenedor se está cayendo

        float tiempoPorFrame = tiempoCaida / 5f;  // Dividimos el tiempo entre los 5 fotogramas
        animator.speed = 0.2f / tiempoPorFrame;  // Ajustamos la velocidad de la animación para que se sincronicen con la caída

        for (int i = 0; i < 5; i++)  // Movemos el contenedor en 5 pasos
        {
            transform.position += Vector3.down * distanciaCaida / 5f;  // Mueve el contenedor hacia abajo
            yield return new WaitForSeconds(tiempoPorFrame);  // Espera entre fotogramas
        }

        animator.speed = 5f;  // Restauramos la velocidad de la animación a su valor original después de la caída
        SeEstaCayendo = false;  // Marcamos que el contenedor ya no se está cayendo
    }

    public void Atrapar(GameObject objetivo)
    {
        // Desactivamos el objeto atrapado
        objetivo.SetActive(false);
        
        if (objetivo.CompareTag("Player"))
        {
            StartCoroutine(EsperarYSonarPerderVida());
            StartCoroutine(ReiniciarJuegoConRetraso());
        }
    }

    private IEnumerator EsperarYSonarPerderVida()
    {
        yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos

        if (loseLifeSound != null)
        {
            AudioManager.PlaySound(loseLifeSound, 0.5f);
            Debug.Log("Sonido de perder vida reproducido.");
        }
    }

    private IEnumerator ReiniciarJuegoConRetraso()
    {
        yield return new WaitForSeconds(3f);  // Esperamos 3 segundos

        // Restamos una vida al jugador
        GameManager.Instance.PerderVida(false);
        
        // Verificamos si el jugador se queda sin vidas
        if (GameManager.Instance.Vidas <= 0)
        {
            // Si no tiene más vidas, mostramos Game Over
            GameManager.Instance.GameOver();
        }
        else
        {
            // Si aún le quedan vidas, reiniciamos la escena
            GameManager.Instance.ReiniciarJuego();
        }
    }
}

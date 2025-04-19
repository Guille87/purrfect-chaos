using System.Collections;
using UnityEngine;

public class ContenedorController : MonoBehaviour
{
    private Animator animator;
    private float tiempoCaida = 0.25f;  // Tiempo total para la caída
    private float distanciaCaida = 1f;  // Distancia que se mueve en cada caída

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

        float tiempoPorFrame = tiempoCaida / 5f;  // Dividimos el tiempo entre los 5 fotogramas
        animator.speed = 0.2f / tiempoPorFrame;  // Ajustamos la velocidad de la animación para que se sincronicen con la caída

        for (int i = 0; i < 5; i++)  // Movemos el contenedor en 5 pasos
        {
            transform.position += Vector3.down * distanciaCaida / 5f;  // Mueve el contenedor hacia abajo
            yield return new WaitForSeconds(tiempoPorFrame);  // Espera entre fotogramas
        }

        animator.speed = 5f;  // Restauramos la velocidad de la animación a su valor original después de la caída
    }
}

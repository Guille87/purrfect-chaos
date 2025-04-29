using System.Collections;
using UnityEngine;

[ExecuteInEditMode] // Permite que el script se ejecute en el modo de edición en Unity
public class PilarController : MonoBehaviour
{
    private Animator animator;
    private GameObject plataformaEncima;
    private GameObject contenedorEncima;
    private bool roto = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        
        Vector3 origen = transform.position + Vector3.up * 0.4f;
        float distancia = 0.5f;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origen, Vector2.up, distancia);

        bool hayPlataforma = false;
        bool hayContenedor = false;

        float distanciaMinima = Mathf.Infinity;
        PlataformaPart parteMasCercana = null;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Plataforma"))
            {
                // Buscar en hijos del objeto golpeado alguno que tenga el script PlataformaPart
                PlataformaPart[] partes = hit.collider.GetComponentsInChildren<PlataformaPart>();
                foreach (PlataformaPart parte in partes)
                {
                    float distanciaVertical = parte.transform.position.y - transform.position.y;
                    float distanciaHorizontal = Mathf.Abs(parte.transform.position.x - transform.position.x);

                    if (distanciaVertical > 0 && distanciaVertical < distanciaMinima && distanciaHorizontal < 0.5f)
                    {
                        distanciaMinima = distanciaVertical;
                        parteMasCercana = parte;
                    }
                }

                // Si no tiene partes, usamos el collider completo (backup)
                if (partes.Length == 0)
                {
                    float distanciaVertical = hit.collider.transform.position.y - transform.position.y;
                    float distanciaHorizontal = Mathf.Abs(hit.collider.transform.position.x - transform.position.x);

                    if (distanciaVertical > 0 && distanciaVertical < distanciaMinima && distanciaHorizontal < 0.5f)
                    {
                        distanciaMinima = distanciaVertical;
                        plataformaEncima = hit.collider.gameObject;
                        hayPlataforma = true;
                    }
                }
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Container"))
            {
                contenedorEncima = hit.collider.gameObject;
                hayContenedor = true;
            }
        }

        // Si encontramos una parte adecuada, la usamos
        if (parteMasCercana != null)
        {
            plataformaEncima = parteMasCercana.gameObject;
            hayPlataforma = true;
        }

        // Dibujar línea según el resultado
        if (hayPlataforma && hayContenedor)
        {
            Debug.DrawLine(origen, origen + Vector3.up * distancia, Color.blue, 15f); // Plataforma y contenedor
        }
        else if (hayPlataforma)
        {
            Debug.DrawLine(origen, origen + Vector3.up * distancia, Color.yellow, 10f); // Solo plataforma
        }
        else
        {
            Debug.DrawLine(origen, origen + Vector3.up * distancia, Color.red, 5f); // Nada encima
        }

        // Verificar si hay un pilar encima en modo edición
        if (Application.isEditor && !Application.isPlaying)
        {
            VerificarPilarEncima();
        }
    }

    private void VerificarPilarEncima()
    {
        // Verificar si hay otro pilar en la misma posición (arriba del actual)
        Collider2D[] coliders = Physics2D.OverlapCircleAll(transform.position + Vector3.up * 1.0f, 0.2f, LayerMask.GetMask("Pilar"));
        
        if (coliders.Length > 0)
        {
            // Si hay un pilar encima, mostramos un aviso en el editor
            Debug.LogWarning("¡No se puede colocar un pilar aquí! Ya hay un pilar encima.", this);
        }
    }
    
    private void OnDrawGizmos()
    {
        // Llamar a la función que verifica si hay un pilar encima
        VerificarPilarEncima();

        // Definir el color del Gizmo
        Gizmos.color = Color.red;

        // Dibujar un círculo en la posición del pilar para visualizar la zona de verificación
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.0f, 0.2f);
    }

    public void Romper()
    {
        if (roto) return;
        roto = true;

        animator.SetTrigger("Romper"); // Pilar_Rotura
    }

    public void DestruirPilar()
    {
        if (contenedorEncima != null)
        {
            contenedorEncima.GetComponent<ContenedorController>()?.Romper();
        }

        if (plataformaEncima != null)
        {
            Destroy(plataformaEncima);
        }

        Destroy(gameObject); // Destruir el Pilar
    }
}

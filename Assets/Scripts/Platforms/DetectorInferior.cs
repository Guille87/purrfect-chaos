using UnityEngine;

public class DetectorInferior : MonoBehaviour
{
    private ContenedorController contenedor;

    private void Awake()
    {
        contenedor = GetComponentInParent<ContenedorController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (contenedor.SeEstaCayendo && (collision.CompareTag("Player") || collision.CompareTag("Enemy")))
        {
            contenedor.Atrapar(collision.gameObject);
        }
    }
}

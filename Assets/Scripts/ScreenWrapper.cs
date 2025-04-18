using UnityEngine;

public class ScreenWrapper : MonoBehaviour
{
    private float screenLeft;
    private float screenRight;

    private float objectWidth;

    void Start()
    {
        Camera cam = Camera.main;

        // Limites de la cámara en coordenadas del mundo
        Vector2 screenBottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector2 screenTopRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        screenLeft = screenBottomLeft.x + 0.5f;
        screenRight = screenTopRight.x - 0.5f;

        // Obtener el ancho del objeto
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            objectWidth = sr.bounds.size.x / 2f;
        }
        else
        {
            objectWidth = 0.5f;
        }
    }

    void Update()
    {
        Vector3 pos = transform.position;

        // Comprobar si el objeto está fuera de la pantalla y moverlo al otro lado
        if (pos.x < screenLeft - objectWidth)
            pos.x = screenRight + objectWidth;
        else if (pos.x > screenRight + objectWidth)
            pos.x = screenLeft - objectWidth;

        transform.position = pos;
    }
}

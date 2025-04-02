#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class PlataformaManager : MonoBehaviour
{
    public GameObject plataformaBasePrefab;
    public GameObject plataformaExtensionPrefab;
    public int extensionesIzquierda = 2;  // Cantidad de plataformas a la izquierda
    public int extensionesDerecha = 2;    // Cantidad de plataformas a la derecha
    public float distanciaPlataformas = 1f;

    private void OnValidate()
    {
        if (!Application.isPlaying && transform.childCount == 0)
        {
            EditorApplication.delayCall += () => {
                if (this != null)
                {
                    GenerarPlataformasEditor();
                }
            };
        }
    }

    void Start()
    {
        // Si ya hay plataformas en la escena, no crear nuevas
        if (transform.childCount == 0)
        {
            GenerarPlataformasRuntime();
        }
    }

    void GenerarPlataformasEditor()
    {
        if (Application.isPlaying) return; // Evita cambios en runtime

        // Eliminar plataformas antiguas (solo si son instancias en la escena)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (!PrefabUtility.IsPartOfPrefabAsset(child)) // Solo destruir si es instancia
            {
                #if UNITY_EDITOR
                DestroyImmediate(child);
                #else
                Destroy(child);
                #endif
            }
        }

        // Crear plataformas SOLO si no hay en la escena
        if (transform.childCount == 0)
        {
            CrearPlataformasEditor();
        }
    }

    void CrearPlataformasEditor()
    {
        // Crear la plataforma base en el centro
        GameObject plataformaBase = Instantiate(plataformaBasePrefab);
        plataformaBase.name = "PlataformaBase";
        plataformaBase.transform.SetParent(transform, false);
        plataformaBase.transform.localPosition = Vector3.zero;

        // Crear plataformas a la izquierda
        for (int i = 1; i <= extensionesIzquierda; i++)
        {
            GameObject nuevaPlataforma = Instantiate(plataformaExtensionPrefab);
            nuevaPlataforma.name = "PlataformaIzquierda_" + i;
            nuevaPlataforma.transform.SetParent(transform, false);
            nuevaPlataforma.transform.localPosition = new Vector3(-i * distanciaPlataformas, 0, 0);
        }

        // Crear plataformas a la derecha
        for (int i = 1; i <= extensionesDerecha; i++)
        {
            GameObject nuevaPlataforma = Instantiate(plataformaExtensionPrefab);
            nuevaPlataforma.name = "PlataformaDerecha_" + i;
            nuevaPlataforma.transform.SetParent(transform, false);
            nuevaPlataforma.transform.localPosition = new Vector3(i * distanciaPlataformas, 0, 0);
        }
    }

    void GenerarPlataformasRuntime()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Crear la plataforma base en el centro
        GameObject plataformaBase = Instantiate(plataformaBasePrefab, transform);
        plataformaBase.name = "PlataformaBase";
        plataformaBase.transform.localPosition = Vector3.zero;

        // Crear plataformas a la izquierda
        for (int i = 1; i <= extensionesIzquierda; i++)
        {
            GameObject nuevaPlataforma = Instantiate(plataformaExtensionPrefab, transform);
            nuevaPlataforma.name = "PlataformaIzquierda_" + i;
            nuevaPlataforma.transform.localPosition = new Vector3(-i * distanciaPlataformas, 0, 0);
        }

        // Crear plataformas a la derecha
        for (int i = 1; i <= extensionesDerecha; i++)
        {
            GameObject nuevaPlataforma = Instantiate(plataformaExtensionPrefab, transform);
            nuevaPlataforma.name = "PlataformaDerecha_" + i;
            nuevaPlataforma.transform.localPosition = new Vector3(i * distanciaPlataformas, 0, 0);
        }
    }
}
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
    public float deteccionCuerdaOffset = 0.7f; // Distancia vertical para detectar cuerdas debajo

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            #if UNITY_EDITOR
            EditorApplication.delayCall += () => {
                if (this != null)
                {
                    GenerarPlataformasEditor();
                }
            };
            #endif
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

#if UNITY_EDITOR
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
        CrearYConfigurarPlataforma(Vector3.zero, "PlataformaBase");

        for (int i = 1; i <= extensionesIzquierda; i++)
        {
            Vector3 pos = new Vector3(-i * distanciaPlataformas, 0, 0);
            CrearYConfigurarPlataforma(pos, "PlataformaIzquierda_" + i);
        }

        for (int i = 1; i <= extensionesDerecha; i++)
        {
            Vector3 pos = new Vector3(i * distanciaPlataformas, 0, 0);
            CrearYConfigurarPlataforma(pos, "PlataformaDerecha_" + i);
        }
    }
#endif

    void GenerarPlataformasRuntime()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        CrearYConfigurarPlataforma(Vector3.zero, "PlataformaBase");

        for (int i = 1; i <= extensionesIzquierda; i++)
        {
            Vector3 pos = new Vector3(-i * distanciaPlataformas, 0, 0);
            CrearYConfigurarPlataforma(pos, "PlataformaIzquierda_" + i);
        }

        for (int i = 1; i <= extensionesDerecha; i++)
        {
            Vector3 pos = new Vector3(i * distanciaPlataformas, 0, 0);
            CrearYConfigurarPlataforma(pos, "PlataformaDerecha_" + i);
        }
    }

    void CrearYConfigurarPlataforma(Vector3 posicion, string nombre)
    {
        GameObject prefab = nombre == "PlataformaBase" ? plataformaBasePrefab : plataformaExtensionPrefab;
        GameObject plataforma = Instantiate(prefab, transform);
        plataforma.name = nombre;
        plataforma.transform.localPosition = posicion;
    }
}
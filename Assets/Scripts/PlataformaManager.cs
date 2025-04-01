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
        if (!Application.isPlaying)
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
        GenerarPlataformasRuntime();
    }

    void GenerarPlataformasEditor()
    {
        // Eliminar plataformas antiguas
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            #if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
            #else
            Destroy(transform.GetChild(i).gameObject);
            #endif
        }

        // Crear la plataforma base en el centro
        GameObject plataformaBase = PrefabUtility.InstantiatePrefab(plataformaBasePrefab, transform) as GameObject;
        plataformaBase.name = "PlataformaBase";
        plataformaBase.transform.localPosition = Vector3.zero;

        // Crear plataformas a la izquierda
        for (int i = 1; i <= extensionesIzquierda; i++)
        {
            GameObject nuevaPlataforma = PrefabUtility.InstantiatePrefab(plataformaExtensionPrefab, transform) as GameObject;
            nuevaPlataforma.name = "PlataformaIzquierda_" + i;
            nuevaPlataforma.transform.localPosition = new Vector3(-i * distanciaPlataformas, 0, 0);
        }

        // Crear plataformas a la derecha
        for (int i = 1; i <= extensionesDerecha; i++)
        {
            GameObject nuevaPlataforma = PrefabUtility.InstantiatePrefab(plataformaExtensionPrefab, transform) as GameObject;
            nuevaPlataforma.name = "PlataformaDerecha_" + i;
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
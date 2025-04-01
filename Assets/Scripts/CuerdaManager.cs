#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CuerdaManager : MonoBehaviour
{
    public GameObject cuerdaPlataformaPrefab; // Prefab de la primera cuerda
    public GameObject cuerdaContinuacionPrefab; // Prefab de las cuerdas de continuación
    public int cantidadCuerdas = 5; // Número de cuerdas a generar
    public float alturaCuerda = 1f; // Distancia entre cuerdas

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            #if UNITY_EDITOR
            // Verifica si el objeto está dentro de un prefab para evitar errores
            if (PrefabUtility.IsPartOfPrefabAsset(this))
            {
                return;
            }

            EditorApplication.delayCall += () => {
                if (this != null) 
                {
                    GenerarCuerdasEditor();
                }
            };
            #endif
        }
    }

    void Start()
    {
        GenerarCuerdasRuntime(); // Generar cuerdas en tiempo de ejecución
    }

    void GenerarCuerdasEditor()
    {
        #if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabAsset(this))
        {
            return; // Evita instanciar dentro de un prefab
        }

        // Eliminar cuerdas antiguas
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Crear las cuerdas en el editor
        for (int i = 0; i < cantidadCuerdas; i++)
        {
            GameObject nuevaCuerda;

            if (i == 0)
            {
                nuevaCuerda = PrefabUtility.InstantiatePrefab(cuerdaPlataformaPrefab) as GameObject;
                nuevaCuerda.name = "CuerdaPlataforma";
            }
            else
            {
                nuevaCuerda = PrefabUtility.InstantiatePrefab(cuerdaContinuacionPrefab) as GameObject;
                nuevaCuerda.name = "CuerdaContinuacion_" + i;
            }

            nuevaCuerda.transform.SetParent(transform);
            nuevaCuerda.transform.localPosition = new Vector3(0, i * alturaCuerda, 0);
        }
        #endif
    }

    void GenerarCuerdasRuntime()
    {
        // Eliminar cuerdas antiguas en tiempo de ejecución
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Crear las cuerdas en tiempo de ejecución
        for (int i = 0; i < cantidadCuerdas; i++)
        {
            GameObject nuevaCuerda;

            if (i == 0)
            {
                nuevaCuerda = Instantiate(cuerdaPlataformaPrefab, transform);
                nuevaCuerda.name = "CuerdaPlataforma";
            }
            else
            {
                nuevaCuerda = Instantiate(cuerdaContinuacionPrefab, transform);
                nuevaCuerda.name = "CuerdaContinuacion_" + i;
            }

            nuevaCuerda.transform.localPosition = new Vector3(0, i * alturaCuerda, 0);
        }
    }
}

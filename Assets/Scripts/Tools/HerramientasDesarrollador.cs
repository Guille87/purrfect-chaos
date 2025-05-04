using UnityEngine;

public class HerramientasDesarrollador : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            PlayerPrefs.DeleteKey("ControlesMostrados");
            PlayerPrefs.Save();
            Debug.Log("Clave 'ControlesMostrados' eliminada.");
        }
    }
}

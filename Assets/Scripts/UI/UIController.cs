using UnityEngine;

public class UIController : MonoBehaviour
{
    public void Jugar()
    {
        // Reiniciar vidas y puntos
        PlayerPrefs.SetInt("Vidas", 3);
        PlayerPrefs.SetInt("Puntos", 0);
        PlayerPrefs.Save();

        SceneTransition fader = FindFirstObjectByType<SceneTransition>();
        if (fader != null)
        {
            fader.TransitionToScene("Level1");
        }
    }

    public void VolverAlMenu()
    {
        SceneTransition fader = FindFirstObjectByType<SceneTransition>();
        if (fader != null)
        {
            fader.TransitionToScene("MenuPrincipal");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private UIController uiController;
    private GameObject gameOverPanel;
    private TextMeshProUGUI vidasText;
    private TextMeshProUGUI puntosText;

    [Header("Gameplay")]
    public int vidasIniciales = 3;
    public int puntuacionVidaExtra = 2500;
    public int maxVidas = 9;

    [Header("Audio")]
    public AudioClip stagesLoop;
    public AudioClip sfxLevelUp;
    public AudioClip sfxLoseLife;
    public AudioClip musicGameOver;
    public AudioClip Victory;
    public AudioClip musicCredits;

    [Header("Pausa")]
    [SerializeField] private PauseManager pauseManager;

    [SerializeField] private List<string> escenasJugables;
    private List<VasoController> vasosEnNivel = new List<VasoController>();

    private int vidas;
    private int puntos;
    private int proximaVidaExtra;
    private bool enTransicion = false; // Para evitar reinicios de música

    public int Vidas => vidas; // Propiedad para acceder a las vidas desde otros scripts

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Para que no se destruya al cambiar de escena
            SceneManager.sceneLoaded += OnSceneLoaded; // Suscribirse al evento de carga de escena
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Si ya existe una instancia, destruye la nueva
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si estamos en el menu principal, destruir el GameManager existente.
        if (scene.name == "MenuPrincipal" && Instance != null)
        {
            Destroy(Instance.gameObject);  // Elimina el GameManager de la escena de menú
            Instance = null;
        }

        // Si estamos en la escena final de créditos
        if (scene.name == "GameCompleted")
        {
            if (musicCredits != null)
            {
                float volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
                AudioManager.PlayMusic(musicCredits, volume);
            }
            return; // Salir sin hacer lo de escenas jugables
        }
        
        // Solo buscar los elementos de UI si estamos en la escena de juego
        if (escenasJugables.Contains(scene.name))
        {
            // Reasigna referencias a los textos
            vidasText = GameObject.Find("TextVidas")?.GetComponent<TextMeshProUGUI>();
            puntosText = GameObject.Find("TextPuntos")?.GetComponent<TextMeshProUGUI>();

            // Busca el panel de Game Over
            gameOverPanel = GameObject.Find("GameOverPanel");

            // Busca el controlador de UI
            uiController = GameObject.Find("MenuManager")?.GetComponent<UIController>();

            vasosEnNivel.Clear();
            vasosEnNivel.AddRange(FindObjectsByType<VasoController>(FindObjectsSortMode.None));

            pauseManager = FindFirstObjectByType<PauseManager>();

            if (stagesLoop != null && !enTransicion)
            {
                float volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
                AudioManager.PlayMusic(stagesLoop, volume);
            }
            else
            {
                Debug.LogWarning("El clip de música 'stagesLoop' no está asignado en el GameManager.");
            }

            OcultarGameOverPanel();

            ActualizarUI();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Desuscribirse del evento al destruir el objeto
    }
    
    void Start()
    {
        // Cargar las vidas y puntos guardados o establecer valores iniciales
        vidas = PlayerPrefs.GetInt("Vidas", vidasIniciales);
        puntos = PlayerPrefs.GetInt("Puntos", 0);

        // Si no tienes vidas, reiniciar a vidasIniciales
        if (vidas <= 0)
        {
            vidas = vidasIniciales;
            puntos = 0;
            PlayerPrefs.SetInt("Vidas", vidas);
            PlayerPrefs.SetInt("Puntos", puntos);
            PlayerPrefs.Save();
        }
        
        proximaVidaExtra = puntuacionVidaExtra;

        ActualizarUI();
    }

    public void SumarPuntos(int cantidad)
    {
        puntos += cantidad;

        if (vidas < maxVidas && puntos >= proximaVidaExtra)
        {
            vidas++;
            proximaVidaExtra += puntuacionVidaExtra;

            if (sfxLevelUp != null)
            {
                float volume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
                AudioManager.PlaySound(sfxLevelUp, volume);
            }
        }

        ActualizarUI();
    }

    public void PerderVida(bool reproducirSonido = true)
    {
        if (vidas <= 0) return;

        vidas--;

        if (reproducirSonido && sfxLoseLife != null)
        {
            float volume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
            AudioManager.PlaySound(sfxLoseLife, volume);
        }

        // Guardar las nuevas vidas y puntos
        PlayerPrefs.SetInt("Vidas", vidas);
        PlayerPrefs.SetInt("Puntos", puntos);
        PlayerPrefs.Save();

        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (vidasText != null)
            vidasText.text = "Vidas: " + vidas;
        
        if (puntosText != null)
            puntosText.text = "Puntos: " + puntos;
    }

    public void VasoRoto(VasoController vaso)
    {
        // Asegurarnos de que la lista está actualizada
        if (!vasosEnNivel.Contains(vaso))
        {
            vasosEnNivel.Clear();
            vasosEnNivel.AddRange(FindObjectsByType<VasoController>(FindObjectsSortMode.None));
        }
        
        vasosEnNivel.Remove(vaso);

        if (vasosEnNivel.Count == 0)
        {
            StartCoroutine(EsperarYPasarNivel());
        }
    }

    private IEnumerator EsperarYPasarNivel()
    {
        enTransicion = true;
        
        if (Victory != null)
        {
            float volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            AudioManager.PlayMusic(Victory, volume, loop: false);
        }

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(Victory.length); // Esperar a que termine la música
        Time.timeScale = 1f;

        PasarAlSiguienteNivel();

        enTransicion = false;
    }

    private void PasarAlSiguienteNivel()
    {
        int escenaActualIndex = SceneManager.GetActiveScene().buildIndex;
        int siguienteEscenaIndex = escenaActualIndex + 1;

        if (siguienteEscenaIndex < SceneManager.sceneCountInBuildSettings)
        {
            string siguienteEscenaName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(siguienteEscenaIndex));
            Debug.Log("Cargando siguiente escena: " + siguienteEscenaName);
            SceneTransition fader = FindFirstObjectByType<SceneTransition>();
            if (fader != null)
            {
                fader.TransitionToScene(siguienteEscenaName);
            }
        }
        else
        {
            Debug.Log("¡Has completado todos los niveles!");
            SceneTransition fader = FindFirstObjectByType<SceneTransition>();
            if (fader != null)
            {
                fader.TransitionToScene("GameCompleted");
            }
        }
    }

    public void IniciarNuevaPartida()
    {
        vidas = 3;
        puntos = 0;
        
        PlayerPrefs.SetInt("Vidas", vidas);
        PlayerPrefs.SetInt("Puntos", puntos);
        PlayerPrefs.Save();
    }

    public void TogglePause()
    {
        if (pauseManager != null)
        {
            pauseManager.TogglePause();
        }
    }

    public void ReiniciarJuego()
    {
        // Guardar el estado de las vidas y los puntos al morir
        PlayerPrefs.SetInt("Vidas", vidas);
        PlayerPrefs.SetInt("Puntos", puntos);
        PlayerPrefs.Save();
        
        // Cargar la escena de nuevo
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OcultarGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f; // Invisible
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }
    }

    public void MostrarGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f; // Visible
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }

    public void GameOver()
    {
        if (gameOverPanel != null)
        {
            MostrarGameOverPanel();
        }

        if (musicGameOver != null)
        {
            float volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            AudioManager.PlayMusic(musicGameOver, volume);
        }

        Time.timeScale = 0f;
    }
}

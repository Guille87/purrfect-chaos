using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TMP_Text vidasText;
    public TMP_Text puntosText;
    public GameObject gameOverPanel;

    [Header("Gameplay")]
    public int vidasIniciales = 3;
    public int puntuacionVidaExtra = 5000;
    public int maxVidas = 9;

    [SerializeField] private List<string> escenasJugables;

    private int vidas;
    private int puntos;
    private int proximaVidaExtra;

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
        // Solo buscar los elementos de UI si estamos en la escena de juego
        if (escenasJugables.Contains(scene.name))
        {
            // Reasigna referencias a los textos
            vidasText = GameObject.Find("TextVidas")?.GetComponent<TMP_Text>();
            puntosText = GameObject.Find("TextPuntos")?.GetComponent<TMP_Text>();

            // Busca el panel de Game Over
            gameOverPanel = GameObject.Find("GameOverPanel");

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
        }

        ActualizarUI();
    }

    public void PerderVida()
    {
        if (vidas <= 0) return;

        vidas--;

        // Guardar las nuevas vidas y puntos
        PlayerPrefs.SetInt("Vidas", vidas);
        PlayerPrefs.SetInt("Puntos", puntos);
        PlayerPrefs.Save();

        ActualizarUI();
    }

    private void ActualizarUI()
    {
        vidasText.text = "Vidas: " + vidas;
        puntosText.text = "Puntos: " + puntos;
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
        Time.timeScale = 0f;
    }
}

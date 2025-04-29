using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI jugarText;
    public TextMeshProUGUI opcionesText;
    public TextMeshProUGUI salirText;
    public TextMeshProUGUI selectorText;

    [Header("Paneles Opciones")]
    public CanvasGroup panelMenuPrincipal;
    public CanvasGroup panelOpciones;

    [Header("Opciones UI")]
    public TextMeshProUGUI musicaText;
    public TextMeshProUGUI fxText;
    public TextMeshProUGUI volverText;
    public Slider musicaSlider;
    public Slider fxSlider;

    [Header("Audio")]
    public AudioClip musicaMenu;

    [Header("SFX")]
    public AudioClip sfx_menu_move;
    public AudioClip sfx_menu_select_in;
    public AudioClip sfx_menu_select_out;
    public AudioClip sfx_menu_select_play;

    private TextMeshProUGUI[] opcionesTexto;
    private TextMeshProUGUI[] textosOpciones;
    private int opcionSeleccionada;
    private bool enOpciones = false;
    private int opcionOpcionesSeleccionada = 0;

    private Vector2 posicionBaseSelector = new Vector2(50, -370);
    private float separacionY = -120f;
    private Vector2 posicionBaseSelectorOpciones = new Vector2(50, -370); // Ajusta según tu layout
    private float separacionYOpciones = -120f;

    private InputSystem_Actions controls;

    void Awake()
    {
        // Inicializar los controles
        controls = new InputSystem_Actions();

        // Activar las acciones del menú
        controls.Menu.Enable();
    }

    void Start()
    {
        // Cargar valores guardados (o por defecto 0.5)
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        musicaSlider.value = savedMusicVolume * 10f; // Escala de 0 a 10
        fxSlider.value = savedSFXVolume * 10f;

        if (musicaMenu != null)
        {
            AudioManager.PlayMusic(musicaMenu, musicaSlider.value / 10f); // Normalizado
        }

        // Añadir todos los textos de opciones al array
        opcionesTexto = new TextMeshProUGUI[] { jugarText, opcionesText, salirText };
        textosOpciones = new TextMeshProUGUI[] { musicaText, fxText, volverText };

        // Hacer que la primera opción esté seleccionada al inicio
        opcionSeleccionada = 0;

        // Inicializar el texto de selección
        UpdateSelectorText();
    }

    void OnEnable()
    {
        // Conectar las acciones de navegación y selección
        controls.Menu.Navigate.performed += ctx => NavegarMenu(ctx.ReadValue<Vector2>());
        controls.Menu.Submit.performed += ctx => Seleccionar();
    }

    void OnDisable()
    {
        // Desconectar las acciones cuando el script se desactiva
        controls.Menu.Navigate.performed -= ctx => NavegarMenu(ctx.ReadValue<Vector2>());
        controls.Menu.Submit.performed -= ctx => Seleccionar();
    }

    private void NavegarMenu(Vector2 direccion)
    {
        if (!enOpciones)
        {
            // Menú principal
            if (direccion.y > 0) CambiarSeleccion(-1);
            else if (direccion.y < 0) CambiarSeleccion(1);
        }
        else
        {
            // Menú de opciones
            if (direccion.y > 0) CambiarSeleccionOpciones(-1);
            else if (direccion.y < 0) CambiarSeleccionOpciones(1);
            else if (direccion.x != 0)
            {
                // Ajuste del slider
                AjustarSlider(direccion.x);
            }
        }
    }

    private void Seleccionar()
    {
        if (enOpciones)
        {
            if (opcionOpcionesSeleccionada == 2) // Volver
            {
                VolverDelMenuOpciones();
            }
            return;
        }
        
        switch (opcionSeleccionada)
        {
            case 0: Jugar(); break;
            case 1: Opciones(); break;
            case 2: Salir(); break;
        }
    }

    private void CambiarSeleccion(int direccion)
    {
        // Cambiar el índice seleccionado
        opcionSeleccionada += direccion;

        // Asegurarse de que el índice esté dentro de los límites
        if (opcionSeleccionada < 0)
            opcionSeleccionada = opcionesTexto.Length - 1;
        if (opcionSeleccionada >= opcionesTexto.Length)
            opcionSeleccionada = 0;
        
        AudioManager.PlaySound(sfx_menu_move);

        // Actualizar el símbolo de selección
        UpdateSelectorText();
    }

    private void CambiarSeleccionOpciones(int direccion)
    {
        opcionOpcionesSeleccionada += direccion;

        if (opcionOpcionesSeleccionada < 0)
            opcionOpcionesSeleccionada = textosOpciones.Length - 1;
        if (opcionOpcionesSeleccionada >= textosOpciones.Length)
            opcionOpcionesSeleccionada = 0;
        
        AudioManager.PlaySound(sfx_menu_move);

        UpdateSeleccionOpciones();
    }

    private void UpdateSeleccionOpciones()
    {
        ColorUtility.TryParseHtmlString("#98FF98", out Color baseColor);

        for (int i = 0; i < textosOpciones.Length; i++)
        {
            textosOpciones[i].color = (i == opcionOpcionesSeleccionada) ? Color.yellow : baseColor;
        }

        // Mover el selector
        Vector2 nuevaPosicion = posicionBaseSelectorOpciones + new Vector2(0, separacionYOpciones * opcionOpcionesSeleccionada);
        selectorText.rectTransform.anchoredPosition = nuevaPosicion;
    }

    private void AjustarSlider(float direccionX)
    {
        float paso = 1f;

        if (opcionOpcionesSeleccionada == 0) // Música
        {
            musicaSlider.value = Mathf.Clamp(musicaSlider.value + paso * Mathf.Sign(direccionX), 0, 10);
            float normalizedVolume = musicaSlider.value / 10f;
            AudioManager.SetMusicVolume(normalizedVolume);
            PlayerPrefs.SetFloat("MusicVolume", normalizedVolume);
        }
        else if (opcionOpcionesSeleccionada == 1) // FX
        {
            fxSlider.value = Mathf.Clamp(fxSlider.value + paso * Mathf.Sign(direccionX), 0, 10);
            float normalizedVolume = fxSlider.value / 10f;
            AudioManager.SetSFXVolume(normalizedVolume);
            PlayerPrefs.SetFloat("SFXVolume", normalizedVolume);

            // Reproducir el sonido de navegación con el nuevo volumen de FX
            AudioManager.PlaySound(sfx_menu_move);
        }

        PlayerPrefs.Save(); // Guardar los cambios
        Debug.Log($"Volumen Música: {musicaSlider.value / 10f}, Volumen FX: {fxSlider.value / 10f}");
    }

    private void UpdateSelectorText()
    {
        // Cambiar el texto del selector
        selectorText.text = ">";

        ColorUtility.TryParseHtmlString("#98FF98", out Color baseColor);

        // Cambiar el color de los textos
        for (int i = 0; i < opcionesTexto.Length; i++)
        {
            if (i == opcionSeleccionada)
            {
                // Color del texto seleccionado
                opcionesTexto[i].color = Color.yellow;
            }
            else
            {
                // Color de los textos no seleccionados
                opcionesTexto[i].color = baseColor;
            }
        }

        // Mover el selector según la opción actual
        Vector2 nuevaPosicion = posicionBaseSelector + new Vector2(0, separacionY * opcionSeleccionada);
        selectorText.rectTransform.anchoredPosition = nuevaPosicion;
    }

    private void Jugar()
    {
        AudioManager.PlaySound(sfx_menu_select_play);

        // Desactivar el action map del menú
        controls.Menu.Disable();

        // Resetear vidas y puntos
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IniciarNuevaPartida();
        }

        // Transición a la escena de juego
        SceneTransition fader = FindFirstObjectByType<SceneTransition>();
        if (fader != null)
        {
            Debug.Log("Iniciando juego...");
            fader.TransitionToScene("Level1");
        }
    }

    private void Opciones()
    {
        enOpciones = true;

        AudioManager.PlaySound(sfx_menu_select_in);
        
        // Panel de opciones visible
        panelOpciones.alpha = 1f;
        panelOpciones.interactable = true;
        panelOpciones.blocksRaycasts = true;

        // Panel principal oculto
        panelMenuPrincipal.alpha = 0f;
        panelMenuPrincipal.interactable = false;
        panelMenuPrincipal.blocksRaycasts = false;

        opcionOpcionesSeleccionada = 0;
        UpdateSeleccionOpciones();
    }

    private void VolverDelMenuOpciones()
    {
        AudioManager.PlaySound(sfx_menu_select_out);

        enOpciones = false;

        // Panel principal visible
        panelMenuPrincipal.alpha = 1f;
        panelMenuPrincipal.interactable = true;
        panelMenuPrincipal.blocksRaycasts = true;

        // Panel de opciones oculto
        panelOpciones.alpha = 0f;
        panelOpciones.interactable = false;
        panelOpciones.blocksRaycasts = false;

        // Volvemos a dejar la opción seleccionada resaltada
        UpdateSelectorText();
    }

    private void Salir()
    {
        AudioManager.PlaySound(sfx_menu_select_out);
        // Salir del juego
        Application.Quit();
    }
}

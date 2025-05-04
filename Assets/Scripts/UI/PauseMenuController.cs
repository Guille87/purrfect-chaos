using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Menú Pausa")]
    public TextMeshProUGUI[] textosPausa; // Reanudar, Opciones, Salir
    public TextMeshProUGUI selectorText;
    public CanvasGroup panelMenuContent;

    [Header("Submenú Opciones")]
    public CanvasGroup panelOpciones;
    public TextMeshProUGUI[] textosOpciones; // Música, FX, Volver
    public Slider musicaSlider;
    public Slider fxSlider;
    public Vector2 posicionBaseSelectorOpciones = new Vector2(820, -435);
    public float separacionYOpciones = -100f;

    [Header("Audio")]
    public AudioClip sfx_menu_move;
    public AudioClip sfx_menu_select_in;
    public AudioClip sfx_menu_select_out;
    
    private int opcionSeleccionada = 0;
    private int opcionOpcionesSeleccionada = 0;
    private bool enOpciones = false;

    private Vector2 posicionBaseSelector = new Vector2(820, -435);
    private float separacionY = -100f;

    private InputSystem_Actions controls;

    void Awake()
    {
        controls = new InputSystem_Actions();
    }

    void Start()
    {
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        musicaSlider.value = savedMusicVolume * 10f;
        fxSlider.value = savedSFXVolume * 10f;
    }

    void Navegar(Vector2 direccion)
    {
        if (!enOpciones)
        {
            if (direccion.y > 0) CambiarSeleccion(-1);
            else if (direccion.y < 0) CambiarSeleccion(1);
        }
        else
        {
            if (direccion.y > 0) CambiarSeleccionOpciones(-1);
            else if (direccion.y < 0) CambiarSeleccionOpciones(1);
            else if (direccion.x != 0) AjustarSlider(direccion.x);
        }
    }

    void Seleccionar()
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
            case 0: Reanudar(); break;
            case 1: Opciones(); break;
            case 2: Salir(); break;
        }
    }

    void CambiarSeleccion(int direccion)
    {
        opcionSeleccionada += direccion;
        if (opcionSeleccionada < 0) opcionSeleccionada = textosPausa.Length - 1;
        if (opcionSeleccionada >= textosPausa.Length) opcionSeleccionada = 0;

        AudioManager.PlayUISound(sfx_menu_move);
        ActualizarSelectorPrincipal();
    }

    private void CambiarSeleccionOpciones(int direccion)
    {
        opcionOpcionesSeleccionada += direccion;
        if (opcionOpcionesSeleccionada < 0) opcionOpcionesSeleccionada = textosOpciones.Length - 1;
        if (opcionOpcionesSeleccionada >= textosOpciones.Length) opcionOpcionesSeleccionada = 0;

        AudioManager.PlayUISound(sfx_menu_move);
        ActualizarSelectorOpciones();
    }

    private void AjustarSlider(float direccionX)
    {
        float paso = 1f;

        if (opcionOpcionesSeleccionada == 0) // Música
        {
            musicaSlider.value = Mathf.Clamp(musicaSlider.value + paso * Mathf.Sign(direccionX), 0, 10);
            float normalized = musicaSlider.value / 10f;
            AudioManager.SetMusicVolume(normalized);
            PlayerPrefs.SetFloat("MusicVolume", normalized);
        }
        else if (opcionOpcionesSeleccionada == 1) // FX
        {
            fxSlider.value = Mathf.Clamp(fxSlider.value + paso * Mathf.Sign(direccionX), 0, 10);
            float normalized = fxSlider.value / 10f;
            AudioManager.SetSFXVolume(normalized);
            PlayerPrefs.SetFloat("SFXVolume", normalized);
            AudioManager.PlayUISound(sfx_menu_move);
        }

        PlayerPrefs.Save();
        Debug.Log($"Volumen Música: {musicaSlider.value / 10f}, Volumen FX: {fxSlider.value / 10f}");
    }

    private void ActualizarSelectorPrincipal()
    {
        ColorUtility.TryParseHtmlString("#98FF98", out Color baseColor);

        for (int i = 0; i < textosPausa.Length; i++)
        {
            textosPausa[i].color = (i == opcionSeleccionada) ? Color.yellow : baseColor;
        }

        Vector2 nuevaPos = posicionBaseSelector + new Vector2(0, separacionY * opcionSeleccionada);
        selectorText.rectTransform.anchoredPosition = nuevaPos;
    }

    private void ActualizarSelectorOpciones()
    {
        ColorUtility.TryParseHtmlString("#98FF98", out Color baseColor);

        for (int i = 0; i < textosOpciones.Length; i++)
        {
            textosOpciones[i].color = (i == opcionOpcionesSeleccionada) ? Color.yellow : baseColor;
        }

        Vector2 nuevaPos = posicionBaseSelectorOpciones + new Vector2(0, separacionYOpciones * opcionOpcionesSeleccionada);
        selectorText.rectTransform.anchoredPosition = nuevaPos;
    }

    public void MostrarMenuPrincipal()
    {
        if (panelMenuContent != null && !panelMenuContent.interactable)
        {
            controls.Menu.Enable();
            controls.Menu.Navigate.performed += ctx => Navegar(ctx.ReadValue<Vector2>());
            controls.Menu.Submit.performed += ctx => Seleccionar();
            Debug.Log("Activando controles del menú");
        }
        
        enOpciones = false;

        panelOpciones.alpha = 0f;
        panelOpciones.interactable = false;
        panelOpciones.blocksRaycasts = false;

        panelMenuContent.alpha = 1f;
        panelMenuContent.interactable = true;
        panelMenuContent.blocksRaycasts = true;

        ActualizarSelectorPrincipal();
    }

    private void Opciones()
    {
        enOpciones = true;

        panelMenuContent.alpha = 0f;
        panelMenuContent.interactable = false;
        panelMenuContent.blocksRaycasts = false;

        panelOpciones.alpha = 1f;
        panelOpciones.interactable = true;
        panelOpciones.blocksRaycasts = true;

        opcionOpcionesSeleccionada = 0;
        ActualizarSelectorOpciones();

        AudioManager.PlayUISound(sfx_menu_select_in);
    }

    private void VolverDelMenuOpciones()
    {
        enOpciones = false;

        panelOpciones.alpha = 0f;
        panelOpciones.interactable = false;
        panelOpciones.blocksRaycasts = false;

        panelMenuContent.alpha = 1f;
        panelMenuContent.interactable = true;
        panelMenuContent.blocksRaycasts = true;

        ActualizarSelectorPrincipal();
        AudioManager.PlayUISound(sfx_menu_select_out);
    }

    private void Reanudar()
    {
        AudioManager.PlayUISound(sfx_menu_select_out);
        CerrarMenu();
        GameManager.Instance.TogglePause();
    }

    public void CerrarMenu()
    {
        // Desactiva controles del menú
        controls.Menu.Navigate.performed -= ctx => Navegar(ctx.ReadValue<Vector2>());
        controls.Menu.Submit.performed -= ctx => Seleccionar();
        controls.Menu.Disable();
        Debug.Log("Controles del menú desactivados");

        // Oculta el panel
        panelMenuContent.alpha = 0f;
        panelMenuContent.interactable = false;
        panelMenuContent.blocksRaycasts = false;

        // Por si acaso, también el submenú
        panelOpciones.alpha = 0f;
        panelOpciones.interactable = false;
        panelOpciones.blocksRaycasts = false;
    }

    private void Salir()
    {
        AudioManager.PlayUISound(sfx_menu_select_out);
        
        controls.Menu.Navigate.performed -= ctx => Navegar(ctx.ReadValue<Vector2>());
        controls.Menu.Submit.performed -= ctx => Seleccionar();
        controls.Menu.Disable();
        Debug.Log("Controles del menú desactivados");

        SceneTransition fader = FindFirstObjectByType<SceneTransition>();
        if (fader != null) fader.TransitionToScene("MenuPrincipal");
    }
}

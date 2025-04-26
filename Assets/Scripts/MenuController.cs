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

    private TextMeshProUGUI[] opcionesTexto;
    private int opcionSeleccionada;

    private Vector2 posicionBaseSelector = new Vector2(50, -370);
    private float separacionY = -120f;

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
        // Añadir todos los textos de opciones al array
        opcionesTexto = new TextMeshProUGUI[] { jugarText, opcionesText, salirText };

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
        // Detectar si la dirección es hacia arriba o hacia abajo
        if (direccion.y > 0) // Arriba
        {
            CambiarSeleccion(-1);
        }
        else if (direccion.y < 0) // Abajo
        {
            CambiarSeleccion(1);
        }
    }

    private void Seleccionar()
    {
        // Acción de selección (simula el click del botón)
        if (opcionSeleccionada == 0)
        {
            Jugar();
        }
        else if (opcionSeleccionada == 1)
        {
            Opciones();
        }
        else if (opcionSeleccionada == 2)
        {
            Salir();
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

        // Actualizar el símbolo de selección
        UpdateSelectorText();
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
        // Abrir las opciones
        Debug.Log("Abrir opciones");
    }

    private void Salir()
    {
        // Salir del juego
        Debug.Log("Salir del juego");
        Application.Quit();
    }
}

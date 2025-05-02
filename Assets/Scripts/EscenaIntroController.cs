using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscenaIntroController : MonoBehaviour
{
    public TextMeshProUGUI textControles; // TextMeshProUGUI para el texto de los controles
    public TextMeshProUGUI textContinuar; // TextMeshProUGUI para el texto de "Pulsa cualquier tecla"
    public float tiempoEspera = 3f; // Tiempo de espera antes de mostrar "Pulsa cualquier tecla"
    private bool esperaTerminado = false;

    private SceneTransition sceneTransition; // Referencia al SceneTransition para manejar la transición de escenas
    private bool keyPressed = false; // Para asegurarse de que solo se presiona una vez
    private CanvasGroup canvasTextosGroup; // CanvasGroup para los textos
    private CanvasGroup canvasFondoGroup; // CanvasGroup para el fondo negro
    private Image fondoImage; // Componente Image para cambiar el color del fondo

    void Start()
    {
        // Obtener el componente SceneTransition
        sceneTransition = FindFirstObjectByType<SceneTransition>();

        // Obtener los componentes CanvasGroup de los Canvas
        canvasTextosGroup = GameObject.Find("CanvasTextos").GetComponent<CanvasGroup>();
        canvasFondoGroup = GameObject.Find("CanvasFondo").GetComponent<CanvasGroup>();

        // Obtener el componente Image para cambiar el color del fondo
        fondoImage = GameObject.Find("CanvasFondo").GetComponentInChildren<Image>();

        // Asegurarnos de que todo el CanvasTextos esté oculto inicialmente
        canvasTextosGroup.alpha = 0f;
        canvasTextosGroup.interactable = false;
        canvasTextosGroup.blocksRaycasts = false;

        // Asegurarnos de que el fondo negro esté visible
        canvasFondoGroup.alpha = 1f;
        canvasFondoGroup.interactable = false;
        canvasFondoGroup.blocksRaycasts = false;

        // Comenzamos inmediatamente con la lógica de la escena
        StartCoroutine(VerificarPrimeraVezYTransitar());
    }

    private IEnumerator VerificarPrimeraVezYTransitar()
    {
        // Esperamos un breve momento para mostrar la pantalla negra
        yield return new WaitForSeconds(0.5f);

        // Verificar si es la primera vez que se muestran los controles
        if (PlayerPrefs.GetInt("ControlesMostrados", 0) == 1)
        {
            // Si ya se han mostrado los controles, cambiamos directamente a MenuPrincipal
            SceneManager.LoadScene("MenuPrincipal");
        }
        else
        {
            // Si es la primera vez, mostramos los controles, cambiamos el color de fondo y damos tiempo al jugador
            MostrarControles();
            CambiarColorFondo();
        }
    }
    private void MostrarControles()
    {
        // Mostrar el contenido del CanvasTextos
        canvasTextosGroup.alpha = 1f;  // Hacemos visible el CanvasTextos
        canvasTextosGroup.interactable = true;  // Permitimos la interacción
        canvasTextosGroup.blocksRaycasts = true;  // Permitimos que el Canvas reciba raycasts

        // Mostrar los controles
        textControles.alpha = 1f; // Asegurarnos de que el texto de los controles sea visible
        textControles.gameObject.SetActive(true); // Activamos el texto de controles

        // Comenzamos la espera para mostrar el mensaje de "Pulsa cualquier tecla"
        StartCoroutine(MostrarMensajeContinuar());
    }

    // Cambiar el color de fondo a #104D3F
    private void CambiarColorFondo()
    {
        if (fondoImage != null)
        {
            fondoImage.color = new Color(16f / 255f, 77f / 255f, 63f / 255f); // #104D3F en formato RGB
        }
    }

    // Coroutine para mostrar el mensaje de "Pulsa cualquier tecla"
    private IEnumerator MostrarMensajeContinuar()
    {
        yield return new WaitForSeconds(tiempoEspera); // Esperamos los 3 segundos

        // Después de esperar, mostramos el texto de continuar
        textContinuar.alpha = 1f; // Hacemos visible el mensaje
        textContinuar.gameObject.SetActive(true); // Activamos el GameObject

        esperaTerminado = true; // Ahora se puede continuar
    }

    void Update()
    {
        // Verificamos si el jugador presiona cualquier tecla para continuar
        if (esperaTerminado && !keyPressed)
        {
            if (AnyKeyOrButtonPressed())
            {
                keyPressed = true; // Prevenimos que se detecte más de una vez

                // Guardamos que ya se han mostrado los controles para que no aparezcan más
                PlayerPrefs.SetInt("ControlesMostrados", 1);
                PlayerPrefs.Save();

                // Usamos el SceneTransition para cambiar la escena
                if (sceneTransition != null)
                {
                    sceneTransition.TransitionToScene("MenuPrincipal");
                }
            }
        }
    }

    // Detecta si se presionó cualquier tecla del teclado o botón del gamepad
    private bool AnyKeyOrButtonPressed()
    {
        // Detecta cualquier tecla de teclado
        if (Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        // Detecta cualquier botón de gamepad
        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                // Verifica si el control es un ButtonControl y si fue presionado este frame
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GameOverInput : MonoBehaviour
{
    private UIController uiController;
    private CanvasGroup gameOverCanvasGroup;
    private bool keyPressed = false;

    void Awake()
    {
        uiController = FindFirstObjectByType<UIController>();
        gameOverCanvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        // Solo escuchar la entrada si el panel de Game Over está visible
        if (gameOverCanvasGroup != null && gameOverCanvasGroup.alpha == 1f && !keyPressed)
        {
            if (AnyKeyOrButtonPressed())
            {
                keyPressed = true;
                VolverAlMenu();
            }
        }
    }

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

    private void VolverAlMenu()
    {
        if (uiController != null)
        {
            Time.timeScale = 1f;
            uiController.VolverAlMenu();
        }
    }
}

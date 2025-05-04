using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GameCompletedInput : MonoBehaviour
{
    private bool keyPressed = false;

    void Update()
    {
        if (!keyPressed && AnyKeyOrButtonPressed())
        {
            keyPressed = true;
            VolverAlMenu();
        }
    }

    private bool AnyKeyOrButtonPressed()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                    return true;
            }
        }

        return false;
    }

    private void VolverAlMenu()
    {
        SceneTransition fader = FindFirstObjectByType<SceneTransition>();
        if (fader != null)
        {
            fader.TransitionToScene("MenuPrincipal");
        }
    }
}

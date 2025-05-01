using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public CanvasGroup pauseCanvasGroup;
    public PauseMenuController pauseMenuController;
    public PlayerInput playerInput;

    [Header("Audio Clips")]
    public AudioClip pauseInClip;
    public AudioClip pauseOutClip;

    private bool isPaused = false;

    private void Start()
    {
        HidePauseMenu();
    }

    private void OnEnable()
    {
        if (!playerInput.enabled)
            playerInput.enabled = true;
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        if (!isPaused)
        {
            Time.timeScale = 0f;
            isPaused = true;
            ShowPauseMenu();
            
            playerInput.SwitchCurrentActionMap("Menu");
            
            // Reproducir el sonido de pausa
            if (pauseInClip != null)
            {
                AudioManager.PlayUISound(pauseInClip);
            }

            AudioManager.PauseMusic();
            AudioListener.pause = true; // Pausar todos los sonidos
        }
    }

    void ResumeGame()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
            HidePauseMenu();

            playerInput.SwitchCurrentActionMap("Player");

            // Reproducir el sonido de reanudación
            if (pauseOutClip != null)
            {
                AudioManager.PlaySound(pauseOutClip);
            }

            AudioManager.ResumeMusic();
            AudioListener.pause = false; // Reanudar todos los sonidos
        }
    }

    void ShowPauseMenu()
    {
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }

        if (pauseMenuController != null)
        {
            pauseMenuController.MostrarMenuPrincipal();
        }
    }

    void HidePauseMenu()
    {
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }
    }
}

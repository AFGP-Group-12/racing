using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button resumeButton;
    private Button exitButton;
    private PlayerInput playerInput;
    private VisualElement pauseMenuOverlay;

    void Start()
    {
        // Get the UI Document
        uiDocument = GetComponent<UIDocument>();
        
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            
            // Get the pause menu overlay
            pauseMenuOverlay = root.Q<VisualElement>("PauseMenuOverlay");
            
            // Get the buttons
            resumeButton = root.Q<Button>("ResumeButton");
            exitButton = root.Q<Button>("ExitButton");
            
            // Register button click events
            if (resumeButton != null)
            {
                resumeButton.clicked += OnResumeClicked;
            }
            
            if (exitButton != null)
            {
                exitButton.clicked += OnExitClicked;
            }
        }
        
        // Get the PlayerInput component
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnResumeClicked()
    {
        // Hide pause menu
        if (pauseMenuOverlay != null)
        {
            pauseMenuOverlay.AddToClassList("hidden");
        }

        // Lock and hide cursor
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        // Switch to Player action map
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Player");
        }
    }

    private void OnExitClicked()
    {
        // Exit the game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnDestroy()
    {
        // Unregister button events to prevent memory leaks
        if (resumeButton != null)
        {
            resumeButton.clicked -= OnResumeClicked;
        }
        
        if (exitButton != null)
        {
            exitButton.clicked -= OnExitClicked;
        }
    }
}

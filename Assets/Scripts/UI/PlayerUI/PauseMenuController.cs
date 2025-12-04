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
    private bool uiInitialized;
    private OptionsMenuController optionsController;
    private VisualElement optionsOverlay;
    private Button optionsCloseButton;
    private Button optionsButton;

    void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        optionsController = GetComponent<OptionsMenuController>();
    }

    void OnEnable()
    {
        RegisterPanelCallbacks();
        TryInitializeUI();
    }

    void OnDisable()
    {
        UnregisterButtonCallbacks();
        ResetUIReferences();
        UnregisterPanelCallbacks();
    }

    private void RegisterPanelCallbacks()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var root = uiDocument.rootVisualElement;
        root.RegisterCallback<AttachToPanelEvent>(OnPanelAttached);
        root.RegisterCallback<DetachFromPanelEvent>(OnPanelDetached);
    }

    private void UnregisterPanelCallbacks()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var root = uiDocument.rootVisualElement;
        root.UnregisterCallback<AttachToPanelEvent>(OnPanelAttached);
        root.UnregisterCallback<DetachFromPanelEvent>(OnPanelDetached);
    }

    private void OnPanelAttached(AttachToPanelEvent evt)
    {
        TryInitializeUI();
    }

    private void OnPanelDetached(DetachFromPanelEvent evt)
    {
        ResetUIReferences();
    }

    private void TryInitializeUI()
    {
        if (uiInitialized || uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        if (root == null || root.panel == null) return;

        pauseMenuOverlay = root.Q<VisualElement>("PauseMenuOverlay");
        resumeButton = root.Q<Button>("ResumeButton");
        exitButton = root.Q<Button>("ExitButton");
        optionsOverlay = root.Q<VisualElement>("optionsOverlay");
        optionsCloseButton = root.Q<Button>("optionsCloseButton");
        optionsButton = root.Q<Button>("OptionsButton");
        

        UnregisterButtonCallbacks();
        RegisterButtonCallbacks();

        uiInitialized = true;
    }

    private void RegisterButtonCallbacks()
    {
        if (resumeButton != null)
            resumeButton.clicked += OnResumeClicked;

        if (exitButton != null)
            exitButton.clicked += OnExitClicked;

        if (optionsButton != null && optionsController != null)
            optionsButton.clicked += OnOptionsClicked;
        else
            Debug.LogWarning("Options button or OptionsMenuController is null in PauseMenuController.");
    }

    private void UnregisterButtonCallbacks()
    {
        if (resumeButton != null)
            resumeButton.clicked -= OnResumeClicked;

        if (exitButton != null)
            exitButton.clicked -= OnExitClicked;

        if (optionsButton != null && optionsController != null)
            optionsButton.clicked -= OnOptionsClicked;
    }

    private void OnResumeClicked()
    {
        // Hide pause menu
        if (pauseMenuOverlay != null)
            pauseMenuOverlay.AddToClassList("hidden");

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

    private void OnOptionsClicked()
    {
        if (optionsController != null)
        {
            Debug.Log("Showing Options Menu from Pause Menu");
            optionsController.Show();
        }
    }

    private void OnDestroy()
    {
        UnregisterPanelCallbacks();
        UnregisterButtonCallbacks();
        ResetUIReferences();
    }

    private void ResetUIReferences()
    {
        uiInitialized = false;
        pauseMenuOverlay = null;
        resumeButton = null;
        exitButton = null;
    }
}

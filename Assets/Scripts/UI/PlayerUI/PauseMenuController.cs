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

    void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
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
    }

    private void UnregisterButtonCallbacks()
    {
        if (resumeButton != null)
            resumeButton.clicked -= OnResumeClicked;

        if (exitButton != null)
            exitButton.clicked -= OnExitClicked;
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

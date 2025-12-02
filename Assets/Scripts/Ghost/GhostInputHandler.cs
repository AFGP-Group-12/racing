using System;
using Messages;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
public class GhostInputHandler : MonoBehaviour
{
    private GhostContext contextScript;

    private PlayerInput input;

    private GhostMovement movementScript;

    private PlayerAbilityManager abilityManager;

    private PlayerStateHandler stateHandler;

    public float horizontalInput { get; private set; }

    public float verticalInput { get; private set; }

    private UIDocument playerUI;
    private VisualElement pauseMenuOverlay;



    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        contextScript = GetComponent<GhostContext>();

        input = contextScript.input;

        movementScript = contextScript.movement;

        // Get the UI Document and pause menu overlay
        playerUI = GetComponent<UIDocument>();
        if (playerUI != null)
        {
            var root = playerUI.rootVisualElement;
            pauseMenuOverlay = root.Q<VisualElement>("PauseMenuOverlay");

            // Ensure pause menu starts hidden
            if (pauseMenuOverlay != null && !pauseMenuOverlay.ClassListContains("hidden"))
            {
                pauseMenuOverlay.AddToClassList("hidden");
            }
        }

        // Start in Player action map
        input.SwitchCurrentActionMap("Player");


        input.actions["Move"].performed += OnMove;
        input.actions["Move"].canceled += OnMoveStop;

        input.actions["Sprint"].started += OnSprint;
        input.actions["Sprint"].canceled += OnSprintEnd;

        var playerActionMap = input.actions.FindActionMap("Player");
        if (playerActionMap != null)
        {
            var pauseAction = playerActionMap.FindAction("Pause");
            if (pauseAction != null)
            {
                pauseAction.started += OnPause;
            }
        }

        // Subscribe to Unpause action from UI action map
        var uiActionMap = input.actions.FindActionMap("UI");
        if (uiActionMap != null)
        {
            var unpauseAction = uiActionMap.FindAction("Unpause");
            if (unpauseAction != null)
            {
                unpauseAction.started += OnUnpause;
            }
        }

        // Ensure only Player action map is enabled at start
        if (playerActionMap != null) playerActionMap.Enable();
        if (uiActionMap != null) uiActionMap.Disable();

    }

    void OnMove(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;
        verticalInput = context.ReadValue<Vector2>().y;

        movementScript.StartMovement(horizontalInput, verticalInput);

    }

    void OnMoveStop(InputAction.CallbackContext context)
    {
        movementScript.StopMovement();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        movementScript.OnSprint();
    }

    private void OnSprintEnd(InputAction.CallbackContext context)
    {
        movementScript.OnSprintEnd();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        Debug.Log("OnPause called");

        // Show pause menu
        if (pauseMenuOverlay != null)
        {
            pauseMenuOverlay.RemoveFromClassList("hidden");
        }

        // Unlock and show cursor
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        // Disable Player action map and enable UI action map
        var playerActionMap = input.actions.FindActionMap("Player");
        var uiActionMap = input.actions.FindActionMap("UI");
        
        if (playerActionMap != null) playerActionMap.Disable();
        if (uiActionMap != null) uiActionMap.Enable();
    }

    private void OnUnpause(InputAction.CallbackContext context)
    {
        Debug.Log("OnUnpause called");

        // Hide pause menu
        if (pauseMenuOverlay != null)
        {
            pauseMenuOverlay.AddToClassList("hidden");
        }

        // Lock and hide cursor
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        // Disable UI action map and enable Player action map
        var playerActionMap = input.actions.FindActionMap("Player");
        var uiActionMap = input.actions.FindActionMap("UI");
        
        if (uiActionMap != null) uiActionMap.Disable();
        if (playerActionMap != null) playerActionMap.Enable();
    }
}

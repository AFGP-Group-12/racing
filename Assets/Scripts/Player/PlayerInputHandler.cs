using System;
using Messages;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerContext contextScript;

    private PlayerInput input;

    private PlayerMovement movementScript;

    private PlayerAbilityManager abilityManager;

    private PlayerStateHandler stateHandler;

    public Ability debugDash; // debugging

    public bool debugGiveDash = false;

    public float horizontalInput { get; private set; }

    public float verticalInput { get; private set; }

    private UIDocument playerUI;
    private VisualElement pauseMenuOverlay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        contextScript = GetComponent<PlayerContext>();

        input = contextScript.input;

        movementScript = contextScript.movement;

        stateHandler = contextScript.stateHandler;
        abilityManager = contextScript.abilityManager;

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

        // Lock cursor at start
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        input.actions["Move"].performed += OnMove;
        input.actions["Move"].canceled += OnMoveStop;

        input.actions["Jump"].started += OnJump;

        input.actions["Sprint"].started += OnSprint;
        input.actions["Sprint"].canceled += OnSprintEnd;

        input.actions["Crouch"].performed += OnSlide;
        input.actions["Crouch"].canceled += OnSlideEnd;

        input.actions["Ability1"].started += OnAbility1;
        input.actions["Ability1"].canceled += OnAbility1End;

        input.actions["Ability2"].started += OnAbility2;
        input.actions["Ability2"].canceled += OnAbility2End;

        input.actions["Ability3"].started += OnAbility3;
        input.actions["Ability3"].canceled += OnAbility3End;

        input.actions["ChangeAbilityIndex"].performed += OnScroll;
        input.actions["ChangeAbilityIndex"].canceled += OnScrollEnd;

        input.actions["ChangeAbility"].started += OnChangeAbility;
        input.actions["ChangeAbility"].canceled += OnChangeAbilityEnd;

        // Subscribe to Pause action from Player action map
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

    void Update()
    {
        if (debugGiveDash)
        {
            abilityManager.debugAdd(debugDash);
            debugGiveDash = false;
        }
    }

    #region Input Functions
    void OnMove(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;
        verticalInput = context.ReadValue<Vector2>().y;

        if (!Mathf.Approximately(horizontalInput, 0) || !Mathf.Approximately(verticalInput, 0))
        {
            movementScript.StartMovement(horizontalInput, verticalInput);
            stateHandler.isWalking = true;
        }
        else
            stateHandler.isWalking = false;

    }

    void OnMoveStop(InputAction.CallbackContext context)
    {
        movementScript.StopMovement();
        stateHandler.isWalking = false;
    }

    void OnJump(InputAction.CallbackContext context)
    {
        movementScript.Jump();
    }

    private void OnSlide(InputAction.CallbackContext context)
    {
        //Debug.Log("Skude opressed");
        movementScript.SlideHeld();
    }
    private void OnSlideEnd(InputAction.CallbackContext context)
    {
        movementScript.SlideReleased();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        movementScript.OnSprint();
    }

    private void OnSprintEnd(InputAction.CallbackContext context)
    {
        movementScript.OnSprintEnd();
    }

    private void OnAbility1(InputAction.CallbackContext context)
    {
        abilityManager.StartAbility1();
    }
    private void OnAbility1End(InputAction.CallbackContext context)
    {
        abilityManager.EndAbility1();
    }

    private void OnAbility2(InputAction.CallbackContext context)
    {
        abilityManager.StartAbility2();
    }
    private void OnAbility2End(InputAction.CallbackContext context)
    {
        abilityManager.EndAbility2();
    }
    private void OnAbility3(InputAction.CallbackContext context)
    {
        abilityManager.StartAbility3();
    }
    private void OnAbility3End(InputAction.CallbackContext context)
    {
        abilityManager.EndAbility3();
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        abilityManager.SwitchActiveIndex((int)context.ReadValue<float>());
    }
    private void OnScrollEnd(InputAction.CallbackContext context)
    {
        abilityManager.SwitchActiveIndex(0);
    }

    private void OnChangeAbility(InputAction.CallbackContext context)
    {
        abilityManager.ChangeAbility(true);
    }
    private void OnChangeAbilityEnd(InputAction.CallbackContext context)
    {
        abilityManager.ChangeAbility(false);
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

    #endregion Input Functions
}
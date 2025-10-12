using System;
using Messages;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerContext contextScript;

    private PlayerInput input;

    private PlayerMovement movementScript;

    private PlayerAbilityManager abilityManager;

    public Ability debugDash; // debugging

    public bool debugGiveDash = false;

    public float horizontalInput { get; private set; }

    public float verticalInput { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        contextScript = GetComponent<PlayerContext>();

        input = contextScript.input;

        movementScript = contextScript.movement;
        abilityManager = contextScript.abilityManager;

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

        movementScript.StartMovement(horizontalInput, verticalInput);
    }

    void OnMoveStop(InputAction.CallbackContext context)
    {
        movementScript.StopMovement();
    }

    void OnJump(InputAction.CallbackContext context)
    {
        movementScript.Jump();
    }

    private void OnSlide(InputAction.CallbackContext context)
    {
        //Debug.Log("Skude opressed");
        movementScript.Slide();
    }
    private void OnSlideEnd(InputAction.CallbackContext context)
    {
        movementScript.SlideEnd();
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
    
    #endregion Input Functions
}

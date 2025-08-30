using System;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed; // Make this private its only like this for debugging purposes
    [SerializeField] float basicSpeed;
    [SerializeField] float sprintSpeed; // Should always be greater than moveSpeed
    [SerializeField] float accelerationIncrement; // Amount the acceleration will be incremented by 
    [SerializeField] float acceleration; // Make this private its only like this for debugging purposes
    [SerializeField] float groundDrag;

    private bool isOnGround;

    [Header("Jump")]
    [SerializeField] float jumpForce;
    [SerializeField] float jumpMultiplier;
    [SerializeField] float jumpCooldown;

    private bool jumpReady;

    // [SerializeField] float currentSpeed; //Debugging purposes

    bool isAccelerating;

    bool isKeepingMomentum;


    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform orientation;
    [SerializeField] float playerHeight;

    [Header("Other Scripts")]
    [SerializeField] PlayerScreenVisuals visualScript;

    private PlayerInput input;

    private float horizontalInput;
    private float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();

        input.actions["Move"].performed += OnMove;
        input.actions["Move"].canceled += OnMoveStop;

        input.actions["Jump"].started += OnJump;

        input.actions["Sprint"].started += OnSprint;
        input.actions["Sprint"].canceled += OnSprintEnd;

        rb.freezeRotation = true;

        jumpReady = true;

        moveSpeed = basicSpeed;

        isAccelerating = false;

    }

    void Update()
    {
        isOnGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        Debug.DrawRay(transform.position, Vector3.down * 1f, Color.green);

        if (isOnGround)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        SpeedControl();
        Accelerate();
        SpeedCheck();
        StopMomentumJump();
        visualScript.SetSpeedVisuals(basicSpeed, sprintSpeed, moveSpeed);
        visualScript.MoveRotation(horizontalInput);

    }


    void FixedUpdate()
    {
        MovePlayer();
    }
    void SpeedCheck()
    {
        // currentSpeed = rb.linearVelocity.magnitude;
    }

    void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;      

        if (isOnGround)
        {
            MovementForce(rb.linearDamping);
        }
        else if (!isOnGround)
        {
            MovementForce(jumpMultiplier);
        }
    }

    void MovementForce(float multiplier)
    {
        moveSpeed = basicSpeed + ((sprintSpeed - basicSpeed) * (acceleration / 100));
    
        // Multiplying the drag means it will only affect the player when they stop holding a movement button
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f * multiplier , ForceMode.Force);
    }


    // Makes sure the movement speed doesnt go over a certain amount
    void SpeedControl()
    {
        Vector3 curVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (curVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVel = curVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }

    }

    void OnMove(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;
        verticalInput = context.ReadValue<Vector2>().y;
    }
    void OnMoveStop(InputAction.CallbackContext context)
    {
        horizontalInput = 0;
        verticalInput = 0;
    }

    void OnJump(InputAction.CallbackContext context)
    {
        if (jumpReady && isOnGround)
        {
            jumpReady = false;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(JumpCooldown), jumpCooldown);
        }
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        accelerationIncrement = math.abs(accelerationIncrement);
        isAccelerating = true;
        isKeepingMomentum = false;
    }

    private void OnSprintEnd(InputAction.CallbackContext context)
    {
        if (isOnGround)
        {
            accelerationIncrement = -math.abs(accelerationIncrement);
            isAccelerating = false;
        }
        else
        {
            isKeepingMomentum = true;
        }
    }

    void StopMomentumJump()
    {
        if (isKeepingMomentum && !isOnGround)
        {
            return;
        }
        else if (isKeepingMomentum && isOnGround)
        {
            accelerationIncrement = -math.abs(accelerationIncrement);
            isAccelerating = false;
        }
    }

    void JumpCooldown()
    {
        jumpReady = true;
    }

    void Accelerate()
    {
        if (isAccelerating && acceleration < 100 && acceleration >= 0)
        {
            acceleration += accelerationIncrement;
        }
        else if (!isAccelerating && acceleration > 0)
        {
            acceleration -= math.abs(accelerationIncrement);
        }
        else if (acceleration >= 100)
        {
            acceleration = 99;
        }
        else if (acceleration < 0)
        {
            acceleration = 0;
        }
    }


}

using System;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{

    #region Variables

    [Header("Movement")]
    [SerializeField] float basicSpeed;
    [SerializeField] float sprintSpeed; // Should always be greater than moveSpeed

    [Tooltip("Determines how quickly the player slows down when they stop moving")]
    [SerializeField] float groundDrag;

    private float acceleration; // Make this private its only like this for debugging purposes
    private float moveSpeed; // Make this private its only like this for debugging purposes
    private float accelerationIncrement = 1f; // Amount the acceleration will be incremented by

    private float horizontalInput;
    private float verticalInput;

    Vector3 moveDirection;

    private bool isOnGround;

    [Header("Jump")]
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;

    [Tooltip("Determines how much movement the player will have while in the air")]
    [SerializeField] float jumpTurningForce;

    private bool jumpReady;

    // [SerializeField] float currentSpeed; //Debugging purposes

    bool isAccelerating;
    bool isKeepingMomentum;


    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform orientation;
    [SerializeField] float playerHeight = 1.96f;

    [Header("Wall Running")]
    [SerializeField] LayerMask wallLayer;

    [Tooltip("How fast you want the player to move when wall running")]
    [SerializeField] float wallRunningForce;

    [Tooltip("How strong you want the initial wall run arc to be")]
    [SerializeField] float wallUpwardForce;

    [Tooltip("Determines how strong the jump off the wall will be in the upwards direction")]
    [SerializeField] float wallJumpForceUp;

    [Tooltip("Determines how strong the jump off the wall will be in the horizontal directions")]
    [SerializeField] float wallJumpForceDirection;

    [Tooltip("Determines how strong the gravity will be while on the wall. Wallrunning disables unity's gravity and uses this instead")]
    [SerializeField] float maxGravityForce;

    public float gravityForce;

    Vector3 wallNormal;
    Vector3 wallForward;
    Vector3 runArc;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    private bool isWallLeft;
    private bool isWallRight;

    [Header("Camera")]
    [Tooltip("Script used to adjust camera visuals. Should be on the main camera game object")]
    [SerializeField] PlayerScreenVisuals visualScript;

    private PlayerInput input;

    private Rigidbody rb;

    [Header("State Machine")]

    private MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        sliding,
        air,
        dashing,
        idle
    }

    private bool walking;
    private bool sprinting;
    private bool wallrunning;
    private bool sliding;
    private bool air;
    private bool dashing;

    #endregion Variables

    #region MonoBehavior

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
        //Debug.DrawRay(transform.position, Vector3.down * 5f, Color.green);

        if (isOnGround)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        // State Handler
        StateHandler();

        // Movement
        SetMovementSpeed();
        SpeedControl();
        Accelerate();
        StopMomentumJump();
        // SpeedCheck(); // For debugging purposes


        // Camera
        visualScript.SetSpeedVisuals(basicSpeed, sprintSpeed, moveSpeed);
        SetCameraRotation();
    }

    void FixedUpdate()
    {
        // Wall Running
        WallRunCheck();


        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.air)
        {
            MovePlayer();
        }
    }

    #endregion MonoBehavior


    #region State Handler
    void StateHandler()
    {

        if (wallrunning)
        {
            state = MovementState.wallrunning;
            WallRun();
        }
        else if (dashing)
        {
            state = MovementState.dashing;
        }
        else if (!isOnGround)
        {
            state = MovementState.air;
        }
        else if (sliding)
        {
            state = MovementState.sliding;
        }
        else if (isOnGround && sprinting)
        {
            state = MovementState.sprinting;
        }
        else
        {
            state = MovementState.walking;
        }
    }

    void SpeedCheck()
    {
        // currentSpeed = rb.linearVelocity.magnitude;
    }

    #endregion State Handler


    #region Basic Movement

    void SetMovementSpeed()
    {
        moveSpeed = basicSpeed + ((sprintSpeed - basicSpeed) * (acceleration / 100));
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
            MovementForce(jumpTurningForce);
        }
    }

    void MovementForce(float multiplier)
    {
        // Multiplying the drag means it will only affect the player when they stop holding a movement button
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f * multiplier, ForceMode.Force);
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
        else if (acceleration <= 0)
        {
            acceleration = 0;
        }
    }

    #endregion Basic Movement Functions


    #region Wall Run Functions

    void WallRunCheck()
    {
        if (!isOnGround)
        {
            Vector3 positionWithOffset = new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z);

            isWallLeft = Physics.Raycast(positionWithOffset, -orientation.right, out leftWallHit, 1f, wallLayer);
            isWallRight = Physics.Raycast(positionWithOffset, orientation.right, out rightWallHit, 1f, wallLayer);

            if (state != MovementState.wallrunning && isWallRight && horizontalInput > 0)
            {
                rb.useGravity = false;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                gravityForce = 0;

                rb.AddForce(new Vector3(0f, wallUpwardForce, 0f), ForceMode.Impulse);

                wallrunning = true;
                //Debug.Log("WallRight");
            }

            if (state != MovementState.wallrunning && isWallLeft && horizontalInput < 0)
            {
                rb.useGravity = false;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                gravityForce = 0;

                rb.AddForce(new Vector3(0f, wallUpwardForce, 0f), ForceMode.Impulse);

                wallrunning = true;
                //Debug.Log("WallLeft");
            }
        }

    }

    void WallRun()
    {

        rb.useGravity = false;

        wallNormal = new Vector3(0, 0, 0);

        wallForward = new Vector3(0, 0, 0);

        runArc = new Vector3(0f, gravityForce, 0f);

        if (isWallRight && state == MovementState.wallrunning)
        {
            wallrunning = true;
            wallNormal = rightWallHit.normal;
            wallForward = Vector3.Cross(wallNormal, transform.up);

            if (Vector3.Dot(wallForward, orientation.forward) < 0)
            {
                wallForward = -wallForward;
            }

            rb.AddForce(wallForward * wallRunningForce + runArc, ForceMode.Force);
            //Debug.Log("WallRight");
        }

        if (isWallLeft && state == MovementState.wallrunning)
        {
            wallrunning = true;
            wallNormal = leftWallHit.normal;
            wallForward = Vector3.Cross(wallNormal, transform.up);

            if (Vector3.Dot(wallForward, orientation.forward) < 0)
            {
                wallForward = -wallForward;
            }

            rb.AddForce(wallForward * wallRunningForce + runArc, ForceMode.Force);
            //Debug.Log("WallLeft");
        }

        if (isOnGround || !isWallLeft && !isWallRight)
        {
            rb.useGravity = true;
            wallrunning = false;
        }

        RunArcDecrease();
    }

    void RunArcDecrease()


    {
        if (gravityForce > -math.abs(maxGravityForce))
        {
            gravityForce -= 0.5f;
        }
    }

    #endregion Wall Run Functions


    #region Jump Functions
    void StopMomentumJump()
    {
        if (isKeepingMomentum && !isOnGround)
        {
            return;
        }
        else if (isKeepingMomentum && isOnGround)
        {
            sprinting = false;
            accelerationIncrement = -math.abs(accelerationIncrement);
            isAccelerating = false;
            isKeepingMomentum = false;
        }
    }

    void SetCameraRotation()
    {
        if (state == MovementState.wallrunning)
        {
            float wallCameraChange = isWallRight ? -1 : 1;
            visualScript.MoveRotation(wallCameraChange);
        }
        else
        {
            visualScript.MoveRotation(horizontalInput);
        }
    }

    void JumpCooldown()
    {
        jumpReady = true;
    }

    #endregion Jump Functions

    
    #region Input Functions
    void OnMove(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;
        verticalInput = context.ReadValue<Vector2>().y;

        if (state == MovementState.sprinting)
        {
            accelerationIncrement = math.abs(accelerationIncrement);
        }
    }

    void OnMoveStop(InputAction.CallbackContext context)
    {
        accelerationIncrement = -math.abs(accelerationIncrement);
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
        if (state == MovementState.wallrunning)
        {
            if (isWallRight)
            {
                wallrunning = false;
                rb.useGravity = true;
                
                if (horizontalInput > 0)
                {
                    horizontalInput = 0;
                }
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(transform.up * wallJumpForceUp + (-orientation.right * wallJumpForceDirection), ForceMode.Impulse);
            }
            else if (isWallLeft)
            {
                wallrunning = false;
                rb.useGravity = true;

                if (horizontalInput < 0)
                {
                    horizontalInput = 0;
                }
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(transform.up * wallJumpForceUp + (orientation.right * wallJumpForceDirection), ForceMode.Impulse);
            }

            Invoke(nameof(JumpCooldown), jumpCooldown);
        }
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        sprinting = true;
        accelerationIncrement = math.abs(accelerationIncrement);
        isAccelerating = true;
        isKeepingMomentum = false;
    }

    private void OnSprintEnd(InputAction.CallbackContext context)
    {
        if (isOnGround)
        {
            sprinting = false;
            accelerationIncrement = -math.abs(accelerationIncrement);
            isAccelerating = false;
        }
        else
        {
            isKeepingMomentum = true;
        }
    }
    
    #endregion Input Functions




}

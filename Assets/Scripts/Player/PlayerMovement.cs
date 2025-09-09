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
    [SerializeField] float jumpTurningForce;
    [SerializeField] float jumpCooldown;

    private bool jumpReady;

    // [SerializeField] float currentSpeed; //Debugging purposes

    bool isAccelerating;

    bool isKeepingMomentum;


    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform orientation;
    [SerializeField] float playerHeight;

    [Header("Wall Running")]

    [SerializeField] float wallRunningForce;

    [SerializeField] float initialRunArcForce;

    [SerializeField] float offWallJumpForce;

    private float gravityForce;

    [SerializeField] float maxGravityForce;

    [SerializeField] float gravityForceDecrement;

    private RaycastHit leftWallHit;

    private RaycastHit rightWallHit;

    private bool isWallLeft;

    private bool isWallRight;

    [Header("Other Scripts")]
    [SerializeField] PlayerScreenVisuals visualScript;

    private float cameraRotationValue;

    private PlayerInput input;

    private float horizontalInput;
    private float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

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
        //Debug.DrawRay(transform.position, Vector3.down * 5f, Color.green);

        if (isOnGround)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        WallRunCheck();
        StateHandler();
        SpeedControl();
        Accelerate();
        // SpeedCheck();
        StopMomentumJump();
        visualScript.SetSpeedVisuals(basicSpeed, sprintSpeed, moveSpeed);
        SetCameraRotation();
    }

    


    void FixedUpdate()
    {
        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.air)
        {
            MovePlayer();
        }
    }

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

    void WallRunCheck()
    {
        Vector3 positionWithOffset = new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z);

        isWallLeft = Physics.Raycast(positionWithOffset, -orientation.right, out leftWallHit, 1f, groundLayer);
        isWallRight = Physics.Raycast(positionWithOffset, orientation.right, out rightWallHit, 1f, groundLayer);

        if (state != MovementState.wallrunning && isWallRight && horizontalInput > 0)
        {
            rb.useGravity = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            gravityForce = 0;

            if (isOnGround)
            {
                rb.AddForce(new Vector3(0f, initialRunArcForce/2, 0f), ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(new Vector3(0f, initialRunArcForce, 0f), ForceMode.Impulse);
            }

            wallrunning = true;
            //Debug.Log("WallRight");
        }

        if (state != MovementState.wallrunning && isWallLeft && horizontalInput < 0)
        {
            rb.useGravity = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            gravityForce = 0;
            
            if (isOnGround)
            {
                rb.AddForce(new Vector3(0f, initialRunArcForce/2, 0f), ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(new Vector3(0f, initialRunArcForce, 0f), ForceMode.Impulse);
            }

            wallrunning = true;
            //Debug.Log("WallLeft");
        }
    }

    void WallRun()
    {

        rb.useGravity = false;

        Vector3 wallNormal = new Vector3(0, 0, 0);

        Vector3 wallForward = new Vector3(0, 0, 0);

        Vector3 runArc = new Vector3(0f, gravityForce, 0f);
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
                rb.AddForce(transform.up * offWallJumpForce + (-orientation.right * offWallJumpForce), ForceMode.Impulse);
            }
            else if (isWallLeft)
            {
                wallrunning = false;
                rb.useGravity = true;
                rb.AddForce(transform.up *  offWallJumpForce + (orientation.right * offWallJumpForce), ForceMode.Impulse);
            }
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


}

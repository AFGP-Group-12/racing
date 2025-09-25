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

    private PlayerContext contextScript;
    private PlayerStateHandler stateHandler;


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
    [SerializeField] float playerHeight = 1.96f;
    private Transform orientation;

    [Header("Wall Running")]
    [SerializeField] LayerMask wallLayer;

    [Tooltip("How fast you want the player to move when wall running")]
    [SerializeField] float wallRunForce;

    [Tooltip("How fast do you want the initial wall boost to be")]
    [SerializeField] float wallBoostForce;

    [Tooltip("How strong you want the initial wall run arc to be")]
    [SerializeField] float wallUpwardForce;

    [Tooltip("Determines how strong the jump off the wall will be in the upwards direction")]
    [SerializeField] float wallJumpForceUp;

    [Tooltip("Determines how strong the jump off the wall will be in the horizontal directions")]
    [SerializeField] float wallJumpForceDirection;

    [Tooltip("Determines how strong the gravity will be while on the wall. Wallrunning disables unity's gravity and uses this instead")]
    [SerializeField] float maxGravityForce;

    private float gravityForce;

    private bool canBoost = false;

    Vector3 wallNormal;
    Vector3 wallForward;
    Vector3 runArc;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    private bool isWallLeft;
    private bool isWallRight;

    [Header("Camera")]
    PlayerScreenVisuals visualScript;

    private PlayerInput input;

    private Rigidbody rb;

    [Header("State Machine")]

    private MovementState state;

    #endregion Variables

    #region MonoBehavior

    void Start()
    {
        contextScript = GetComponent<PlayerContext>();
        stateHandler = contextScript.stateHandler;

        rb = contextScript.rb;
        input = contextScript.input;
        visualScript = contextScript.screenVisuals;



        rb.freezeRotation = true;

        jumpReady = true;

        moveSpeed = basicSpeed;

        isAccelerating = false;

        orientation = contextScript.orintation;

    }

    void Update()
    {
        isOnGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        stateHandler.isOnGround = isOnGround;

        state = stateHandler.state;

        //Debug.DrawRay(transform.position, Vector3.down * 5f, Color.green);

        if (isOnGround)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        // Movement
        SetMovementSpeed();
        SpeedControl();
        Accelerate();
        StopMomentumJump();
        // SpeedCheck(); // For debugging purposes

        // Wall Running
        WallRunCheck();


    }

    void FixedUpdate()
    {
        // Camera
        visualScript.SetSpeedVisuals(basicSpeed, sprintSpeed, moveSpeed);
        SetCameraRotation();


        if (state == MovementState.wallrunning)
        {
            WallRun();
        }

        else if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.air)
        {
            MovePlayer();
        }
    }

    #endregion MonoBehavior


    #region Basic Movement

    public void StartMovement(float horizontalInput, float verticalInput)
    {
        this.horizontalInput = horizontalInput;
        this.verticalInput = verticalInput;

        if (state == MovementState.sprinting)
        {
            accelerationIncrement = math.abs(accelerationIncrement);
        }
    }

    public void StopMovement()
    {
        accelerationIncrement = -math.abs(accelerationIncrement);
        horizontalInput = 0;
        verticalInput = 0;
    }

    public void OnSprint()
    {
        stateHandler.isSprinting = true;
        accelerationIncrement = math.abs(accelerationIncrement);
        isAccelerating = true;
        isKeepingMomentum = false;
    }

    public void OnSprintEnd()
    {
        if (isOnGround)
        {
            stateHandler.isSprinting = false;
            accelerationIncrement = -math.abs(accelerationIncrement);
            isAccelerating = false;
        }
        else
        {
            isKeepingMomentum = true;
        }
    }

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

    #region Jump Functions

    public void Jump()
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
                stateHandler.isWallrunning = false;
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
                stateHandler.isWallrunning = false;
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

    void StopMomentumJump()
    {
        if (isKeepingMomentum && !isOnGround)
        {
            return;
        }
        else if (isKeepingMomentum && isOnGround)
        {
            stateHandler.isSprinting = false;
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

                canBoost = true;
                stateHandler.isWallrunning = true;
                //Debug.Log("WallRight");
            }

            if (state != MovementState.wallrunning && isWallLeft && horizontalInput < 0)
            {
                rb.useGravity = false;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                gravityForce = 0;

                rb.AddForce(new Vector3(0f, wallUpwardForce, 0f), ForceMode.Impulse);

                canBoost = true;
                stateHandler.isWallrunning = true;
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
            stateHandler.isWallrunning = true;
            wallNormal = rightWallHit.normal;
            wallForward = Vector3.Cross(wallNormal, transform.up);

            if (Vector3.Dot(wallForward, orientation.forward) < 0)
            {
                wallForward = -wallForward;
            }

            if (canBoost)
            {
                rb.AddForce(wallForward * wallBoostForce, ForceMode.Impulse);
                canBoost = false;
            }
            rb.AddForce(wallForward * wallRunForce + runArc, ForceMode.Force);
            //Debug.Log("WallRight");
        }

        if (isWallLeft && state == MovementState.wallrunning)
        {
            stateHandler.isWallrunning = true;
            wallNormal = leftWallHit.normal;
            wallForward = Vector3.Cross(wallNormal, transform.up);

            if (Vector3.Dot(wallForward, orientation.forward) < 0)
            {
                wallForward = -wallForward;
            }

            if (canBoost)
            {
                rb.AddForce(wallForward * wallBoostForce, ForceMode.Impulse);
                canBoost = false;
            }
            rb.AddForce(wallForward * wallRunForce + runArc, ForceMode.Force);
            //Debug.Log("WallLeft");
        }

        if (isOnGround || !isWallLeft && !isWallRight)
        {
            rb.useGravity = true;
            stateHandler.isWallrunning = false;
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


    


}

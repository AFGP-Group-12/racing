using System;
using System.Text.RegularExpressions;
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
    private CapsuleCollider playerCollider;


    [Header("Movement")]
    [SerializeField] float basicSpeed;
    [SerializeField] float sprintSpeed; // Should always be greater than moveSpeed
    [SerializeField] float maxSpeed; // The max speed. This accounts for gaining speed while in air which would be faster than the sprint speed. This is only used for the screen visuals

    private float currentSpringStrength;
    private float currentDamperStrength;
    private float floatHeight = 5f;

    [Tooltip("Determines how quickly the player slows down when they stop moving")]
    [SerializeField] float groundDrag;

    private float acceleration; // Make this private its only like this for debugging purposes
    private float moveSpeed; // Make this private its only like this for debugging purposes
    private float accelerationIncrement = 2f; // Amount the acceleration will be incremented by
    private float horizontalInput;
    private float verticalInput;
    private bool isOnGround;
    Vector3 moveDirection;

    [Header("Float Capsule Movement")]
    [SerializeField] float setFloatHeight = 4.5f;
    [SerializeField] float groundDetectionHeight = 6f;
    [SerializeField] float springStrength = 50f;
    [SerializeField] float damperStrength = 5f;
    [SerializeField] float downwardForceOnSlope = 5f;
    [SerializeField] float upwardForceOnSlope = 3f;

    [Header("Jump")]
    [SerializeField] float jumpForce;

    [Tooltip("Determines how much movement the player will have while in the air")]
    [SerializeField] float jumpTurningForce;

    [SerializeField] float coyoteJumpWindow;

    private float compressedJumpForce;
    private float coyoteJumpTimer;
    private bool coyoteJumpReady;
    private float jumpBuffer = 0.5f;
    private float currentJumpBuffer = 0f;
    // [SerializeField] float currentSpeed; //Debugging purposes
    private bool isAccelerating;
    private bool isKeepingMomentum;


    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float playerHeight = 1.96f;
    private Transform orientation;



    [Header("Slide")]

    [SerializeField] float initialSlideForce;
    [SerializeField] float constantSlideForce;
    [SerializeField] float maxSlideForce;
    [SerializeField] float slideDuration;
    [SerializeField] float slideCooldown;
    [SerializeField] float slideMaxSpeed;
    [SerializeField] float slideFloatHeight = 1;
    [SerializeField] float slideColliderHeight = 1.5f;


    private float currentSlideForce;
    private float slideMinSpeed = 5f;
    private float slideTimer = 0f;
    private float normalColliderHeight;
    private bool slideReady;
    private bool slideHeld;



    [Header("Wall Running")]
    [SerializeField] LayerMask wallLayer;

    [Tooltip("How fast you want the player to move when wall running")]
    [SerializeField] float wallRunForce;

    [Tooltip("How strong you want the initial wall run arc to be")]
    [SerializeField] float wallUpwardForce;

    [Tooltip("Determines how strong the jump off the wall will be in the upwards direction")]
    [SerializeField] float wallJumpForceUp;

    [Tooltip("Determines how strong the jump off the wall will be in the horizontal directions")]
    [SerializeField] float wallJumpForceDirection;

    [Tooltip("Determines how strong the gravity will be while on the wall. Wallrunning disables unity's gravity and uses this instead")]
    [SerializeField] float maxGravityForce;

    [SerializeField] float wallFovChange;

    public float gravityForce;

    Vector3 wallNormal;
    Vector3 wallForward;
    Vector3 runArc;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    private bool isWallLeft;
    private bool isWallRight;

    [Header("Gapple Movement")]

    [SerializeField] float movementDivider;

    private float airEntryMaxSpeed = float.PositiveInfinity;

    [Header("Camera")]
    PlayerScreenVisuals visualScript;

    private PlayerInput input;

    private Rigidbody rb;

    [Header("State Machine")]

    private MovementState state = MovementState.walking;
    private MovementState lastState;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    #endregion Variables

    #region MonoBehavior

    void Start()
    {
        contextScript = GetComponent<PlayerContext>();
        stateHandler = contextScript.stateHandler;
        playerCollider = contextScript.playerObject.GetComponent<CapsuleCollider>();
        // originalPlayerColliderHeight = playerCollider.height;
        normalColliderHeight = playerCollider.height;

        floatHeight = setFloatHeight;

        currentSlideForce = constantSlideForce;

        rb = contextScript.rb;
        input = contextScript.input;
        visualScript = contextScript.screenVisuals;

        rb.freezeRotation = true;

        currentSpringStrength = springStrength;
        currentDamperStrength = damperStrength;
        coyoteJumpReady = false;

        moveSpeed = basicSpeed;
        isAccelerating = false;
        orientation = contextScript.orientation;

        stateHandler.isSliding = false;

        slideHeld = false;
        slideReady = true;

        Time.timeScale = 1;

        // slideTimer = 0f;
    }

    void FixedUpdate()
    {

        isOnGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        if (isOnGround && currentJumpBuffer <= 0f)
        {
            stateHandler.isJumping = false;
        }
        
        
        //Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f) , Color.blue);
        
        stateHandler.isOnGround = isOnGround;
        lastState = state;
        state = stateHandler.state;

        //Debug.DrawRay(transform.position, Vector3.down * 5f, Color.green);

        if(state == MovementState.sliding)
        {
            rb.linearDamping = 1f;
        }
        else if (isOnGround)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        // Movement
        SetMovementSpeed();
        SprintCheck();
        Accelerate();
        JumpBuffer();
        CoyoteTimerCheck();

        // Slide
        SlideCheck();

        // Wall Running
        WallRunCheck();

        if (slideTimer > 0f)
        {
            slideTimer -= Time.deltaTime;
        }


        // Camera
        visualScript.SetSpeedVisuals(basicSpeed, maxSpeed, new Vector3(rb.linearVelocity.x,0,rb.linearVelocity.z).magnitude, state);
        SetCameraRotation();
        
        // State Based Movement
        if (state == MovementState.walking || state == MovementState.sprinting ||  state == MovementState.sliding || state == MovementState.idle)
        {
            FloatPlayer();
        }

        if (state == MovementState.wallrunningright || state == MovementState.wallrunningleft)
        {
            WallRun();
        }

        else if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.air  || state == MovementState.grappling)
        {
            MovePlayer();
        }

        // Speed Control
        SetAirExitSpeed();
        SpeedControl();
    }



    #endregion MonoBehavior


    #region Basic Movement

    void FloatPlayer()
    {

        Ray ray = new Ray(transform.position, Vector3.down);
        //Debug.DrawRay(transform.position + (orientation.forward * 0.65f) , Vector3.down, Color.red);
        //Debug.DrawRay(transform.position + (orientation.forward * 0.35f), Vector3.down, Color.red);

        // This is how you lower the detection rays if they are needed
        // Ray rayFront = new Ray(transform.position + (orientation.forward * 0.65f) - (Vector3.up * 0.2f), Vector3.down);
        // Ray rayBack = new Ray(transform.position + (orientation.forward * 0.35f) - (Vector3.up * 0.2f), Vector3.down);

        Ray rayFront = new Ray(transform.position + (orientation.forward * 0.65f) , Vector3.down);
        Ray rayBack = new Ray(transform.position + (orientation.forward * 0.35f) , Vector3.down);

        // Slide movement based on the float collider
        if (state == MovementState.sliding && Physics.Raycast(ray, out RaycastHit slideHit, groundDetectionHeight, groundLayer))
        {
            if (Physics.Raycast(rayFront, out RaycastHit frontHit, 1.5f, groundLayer) && Physics.Raycast(rayBack, out RaycastHit backHit, 1.5f, groundLayer))
            {
                if (frontHit.distance > backHit.distance)
                {
                    // Debug.Log(frontHit.distance + " > " + backHit.distance);
                    rb.AddForce(orientation.forward * currentSlideForce, ForceMode.Force);
                    rb.AddForce(Vector3.down * downwardForceOnSlope, ForceMode.Impulse);

                    if (currentSlideForce > maxSlideForce)
                    {
                        currentSlideForce = maxSlideForce;
                    }
                    else
                    {
                        currentSlideForce += 0.2f;
                    }
                }
                else if (frontHit.distance < backHit.distance)
                {
                    // Debug.Log(frontHit.distance + " > " + backHit.distance);
                    rb.AddForce(Vector3.up * upwardForceOnSlope, ForceMode.Impulse);
                }
            }
            FloatVelocity(slideHit);
        }

        // Ground movement based on float capsule
        else if (isOnGround && Physics.Raycast(ray, out RaycastHit hit, floatHeight, groundLayer))
        {

            if ((horizontalInput != 0 || verticalInput != 0) && Physics.Raycast(rayFront, out RaycastHit frontHit, 1.5f, groundLayer) && Physics.Raycast(rayBack, out RaycastHit backHit, 1.5f, groundLayer))
            {

                if (frontHit.distance > backHit.distance)
                {
                    // Debug.Log(frontHit.distance + " > " + backHit.distance);
                    float difference = frontHit.distance - backHit.distance;
                    difference /= 0.35f;
                    difference = math.clamp(difference, 0, 1);
                    difference = math.lerp(0, downwardForceOnSlope, difference);
                    rb.AddForce(Vector3.down * difference, ForceMode.Impulse);
                }
                else if (frontHit.distance < backHit.distance)
                {
                    // Debug.Log(frontHit.distance + " < " + backHit.distance);
                    float difference = backHit.distance - frontHit.distance;
                    difference /= 0.35f;
                    difference = math.clamp(difference, 0, 1);
                    difference = math.lerp(0, upwardForceOnSlope, difference);
                    rb.AddForce(Vector3.down * difference, ForceMode.Impulse);
                }
            }

            FloatVelocity(hit);
        }
    }
    
    private void FloatVelocity(RaycastHit hit)
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 rayDirection = transform.TransformDirection(Vector3.down);

        Vector3 otherVelocity = Vector3.zero;
        Rigidbody hitbody = hit.rigidbody;
        if (hitbody != null)
        {
            otherVelocity = hitbody.linearVelocity;
        }
            
        float rayDirectionVelocity = Vector3.Dot(rayDirection, velocity);
        float otherDirectionVelocity = Vector3.Dot(rayDirection, otherVelocity);

        float relativeVelocity = rayDirectionVelocity - otherDirectionVelocity;

        float x = hit.distance - (floatHeight * 0.3f);
        float springForce = (x * currentSpringStrength) - (relativeVelocity * currentDamperStrength);
        if (-springForce > 20f)
        {
            float differenceForce = ((-springForce) - 20f) / 20f;
            float clamp = Mathf.Clamp(0f, 1f, differenceForce);
            compressedJumpForce = Mathf.Lerp(jumpForce * 0.8f, jumpForce * 1.3f, clamp);
        }
        else
        {
            compressedJumpForce = 0f;
        }

        //Debug.DrawRay(transform.position, Vector3.down * floatHeight , Color.red);

        rb.AddForce(rayDirection * springForce, ForceMode.Acceleration);

        if (hitbody != null)
        {
            hitbody.AddForceAtPosition(rayDirection * -springForce, hit.point, ForceMode.Acceleration);
        }
    }

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
    }

    public void OnSprintEnd()
    {
        stateHandler.isSprinting = false;
    }
    
    private void SprintCheck()
    {
        if (stateHandler.isSprinting == true && isOnGround)
        {
            accelerationIncrement = math.abs(accelerationIncrement);
            isAccelerating = true;
        }
        else if(stateHandler.isSprinting == false && isOnGround)
        {
            accelerationIncrement = -math.abs(accelerationIncrement);
            isAccelerating = false;      
        }
    }
    void SetMovementSpeed()
    {
        moveSpeed = basicSpeed + ((sprintSpeed - basicSpeed) * (acceleration / 100));
    }

    void MovePlayer()
    {
        if (state == MovementState.grappling)
        {
            moveDirection = (moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput) / movementDivider;
        }
        else
        {
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        }

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

        //float max = (state == MovementState.air) ? airEntryMaxSpeed : moveSpeed;

        float max = moveSpeed;
        if (state == MovementState.air)
        {
            max = airEntryMaxSpeed;
        }
        else if (state == MovementState.sliding)
        {
            max = slideMaxSpeed;
        }

        if (curVelocity.magnitude > max)
        {
            Vector3 limitedVel = curVelocity.normalized * max;
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

    void SetAirExitSpeed()
    {
        // Only when we just switched to air this frame
        if (lastState != MovementState.air && state == MovementState.air)
        {
            Vector3 hv = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            airEntryMaxSpeed = Mathf.Max(hv.magnitude, moveSpeed);
        }
    }

    #endregion Basic Movement Functions

    #region Jump Functions

    private void JumpBuffer()
    {
        if (currentJumpBuffer > 0f)
        {
            currentSpringStrength = 0f;
            currentDamperStrength = 0f;
            currentJumpBuffer -= Time.deltaTime;
        }
        else
        {
            currentSpringStrength = springStrength;
            currentDamperStrength = damperStrength;
            currentJumpBuffer = 0f;
        }
    }
    private void CoyoteTimerCheck()
    {
        if (isOnGround)
        {
            coyoteJumpReady = false;
            coyoteJumpTimer = coyoteJumpWindow;
        }

        if (currentJumpBuffer > 0 || !isOnGround && coyoteJumpTimer <= 0)
        {
            coyoteJumpReady = false;
            coyoteJumpTimer = 0;
        }
        else if (!isOnGround && coyoteJumpTimer > 0)
        {
            coyoteJumpReady = true;
            coyoteJumpTimer -= Time.deltaTime;
        }
    }

    public void Jump()
    {
        if (state == MovementState.sliding || (state == MovementState.sliding && coyoteJumpReady))
        {
            // This is a slide jump
            currentJumpBuffer = jumpBuffer;

            currentSpringStrength = 0f;
            currentDamperStrength = 0f;

            coyoteJumpReady = false;
            coyoteJumpTimer = 0;
            //currentSpringStrength = 0f;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            stateHandler.isJumping = true;
            float forwardForce = currentSlideForce * 0.05f;
            float jumpForceMultiplier = Mathf.Lerp(1f, 1.4f, currentSlideForce / maxSlideForce);

            SlideEnd(); // Ends a slide if it is currently happening

            rb.AddForce(orientation.forward * forwardForce, ForceMode.Impulse);

            if (compressedJumpForce > 0)
            {
                rb.AddForce(transform.up * compressedJumpForce * jumpForceMultiplier, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(transform.up * jumpForce * jumpForceMultiplier, ForceMode.Impulse);
            }

        }
        else if (isOnGround || coyoteJumpReady)
        {
            currentJumpBuffer = jumpBuffer;

            currentSpringStrength = 0f;
            currentDamperStrength = 0f;
            //currentSpringStrength = 0f;

            coyoteJumpReady = false;
            coyoteJumpTimer = 0;

            stateHandler.isJumping = true;

            SlideEnd(); // Ends a slide if it is currently happening

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (compressedJumpForce > 0)
            {
                rb.AddForce(transform.up * compressedJumpForce, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
        }
        
        if (state == MovementState.wallrunningright || state == MovementState.wallrunningleft)
        {
            if (isWallRight)
            {
                stateHandler.isWallrunningLeft = false;
                stateHandler.isWallrunningRight = false;
                rb.useGravity = true;

                airEntryMaxSpeed = sprintSpeed;
                stateHandler.isJumping = true;

                if (horizontalInput > 0)
                {
                    horizontalInput = 0;
                }
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(transform.up * wallJumpForceUp + (-orientation.right * wallJumpForceDirection), ForceMode.Impulse);
            }
            else if (isWallLeft)
            {
                stateHandler.isWallrunningLeft = false;
                stateHandler.isWallrunningRight = false;
                rb.useGravity = true;

                airEntryMaxSpeed = sprintSpeed;
                stateHandler.isJumping = true;

                if (horizontalInput < 0)
                {
                    horizontalInput = 0;
                }
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(transform.up * wallJumpForceUp + (orientation.right * wallJumpForceDirection), ForceMode.Impulse);
            }
        }
    }

    void SetCameraRotation()
    {
        if (state == MovementState.wallrunningright || state == MovementState.wallrunningleft)
        {
            float wallCameraChange = isWallRight ? -1 : 1;
            visualScript.MoveRotation(wallCameraChange, false, 0f);
        }
        else if (state == MovementState.sliding)
        {
            visualScript.MoveRotation(1 , true , 2.5f);
        }
        else
        {
            visualScript.MoveRotation(horizontalInput , false , 0f);
        }
    }

    void SlideCooldown()
    {
        //slideReady = true;
    }

    public void PogoJump(float pogoJumpForce)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * pogoJumpForce, ForceMode.Impulse);
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

            if (state != MovementState.wallrunningright && state != MovementState.wallrunningleft && isWallRight && horizontalInput > 0)
            {
                rb.useGravity = false;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                gravityForce = 0;

                rb.AddForce(new Vector3(0f, wallUpwardForce, 0f), ForceMode.Impulse);

                stateHandler.isWallrunningRight = true;
                stateHandler.isWallrunningLeft = false;
                //Debug.Log("WallRight");
            }

            if (state != MovementState.wallrunningright && state != MovementState.wallrunningleft  && isWallLeft && horizontalInput < 0)
            {
                rb.useGravity = false;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                gravityForce = 0;

                rb.AddForce(new Vector3(0f, wallUpwardForce, 0f), ForceMode.Impulse);

                stateHandler.isWallrunningRight = false;
                stateHandler.isWallrunningLeft = true;
                //Debug.Log("WallLeft");
            }
        }

    }

    void WallRun()
    {

        rb.useGravity = false;

        wallNormal = new Vector3(0, 0, 0);

        wallForward = new Vector3(0, 0, 0);

        runArc = new Vector3(rb.linearVelocity.x, gravityForce, rb.linearVelocity.z);

        if (isWallRight && state == MovementState.wallrunningright)
        {
            stateHandler.isWallrunningRight = true;
            wallNormal = rightWallHit.normal;
            wallForward = Vector3.Cross(wallNormal, transform.up);

            if (Vector3.Dot(wallForward, orientation.forward) < 0)
            {
                wallForward = -wallForward;
            }

            rb.AddForce((wallForward * wallRunForce) + runArc, ForceMode.Force);
            //Debug.Log("WallRight");
        }

        if (isWallLeft && state == MovementState.wallrunningleft)
        {
            stateHandler.isWallrunningLeft = true;
            wallNormal = leftWallHit.normal;
            wallForward = Vector3.Cross(wallNormal, transform.up);

            if (Vector3.Dot(wallForward, orientation.forward) < 0)
            {
                wallForward = -wallForward;
            }
            rb.AddForce((wallForward * wallRunForce) + runArc, ForceMode.Force);
            //Debug.Log("WallLeft");
        }

        if (isOnGround || !isWallLeft && !isWallRight)
        {
            rb.useGravity = true;
            stateHandler.isWallrunningRight = false;
            stateHandler.isWallrunningLeft = false;
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
    public float GetWallRunFovChange()
    {
        return wallFovChange;
    }

    #endregion Wall Run Functions

    #region Slide Functions

    public void SlideHeld()
    {
        slideHeld = true;
    }
    public void SlideReleased()
    {
        slideHeld = false;
    }

    private void Slide()
    {
        //Debug.Log("Slide Started");
        floatHeight = slideFloatHeight;
        playerCollider.height = slideColliderHeight;
        rb.AddForce(orientation.forward * initialSlideForce, ForceMode.Impulse);
        stateHandler.isSliding = true;
        slideMinSpeed = moveSpeed * 0.6f;
        slideTimer = slideCooldown; // reset cooldown
        slideReady = false;
        // slideRoutine = StartCoroutine(SlideCoroutine());
    }

    private void SlideCheck()
    {
        Vector3 curVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (state == MovementState.sliding && curVelocity.magnitude < slideMinSpeed)
        {
            SlideEnd();
        }

        // Keeping this in case i need it later
        // else if (state != MovementState.sliding)
        // {
        //     Debug.Log("End Slide 2");
        //     SlideEnd();
        // }

        if (slideHeld && !stateHandler.isSliding && isOnGround && slideTimer <= 0f && slideReady)
        {
            Slide();
        }
        else if (!slideHeld && stateHandler.isSliding)
        {
            SlideEnd();
        }
    }
    
    private void SlideEnd()
    {
        //Debug.Log("Slide Ended");
        slideHeld = false;
        floatHeight = setFloatHeight;
        playerCollider.height = normalColliderHeight;
        currentSlideForce = constantSlideForce;
        stateHandler.isSliding = false;
        slideReady = true;
        // StopCoroutine(slideRoutine);
    }

    #endregion Slide Functions
}

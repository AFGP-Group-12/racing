using System;
using System.Text.RegularExpressions;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GhostMovement : MonoBehaviour
{

    #region Variables

    private GhostContext contextScript;
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
    private bool isSprinting;
    Vector3 moveDirection;

    private Transform orientation;

    private Transform Camera;

    



    [Header("Camera")]
    PlayerScreenVisuals visualScript;

    private PlayerInput input;

    private Rigidbody rb;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    #endregion Variables

    #region MonoBehavior

    void Start()
    {
        contextScript = GetComponent<GhostContext>();
        playerCollider = contextScript.playerObject.GetComponent<CapsuleCollider>();
        // originalPlayerColliderHeight = playerCollider.height;

        rb = contextScript.rb;
        input = contextScript.input;

        rb.freezeRotation = true;

        moveSpeed = basicSpeed;

        orientation = contextScript.orientation;
        Camera = contextScript.cameraTransform;


        Time.timeScale = 1;

        // slideTimer = 0f;
    }

    void FixedUpdate()
    {

        rb.linearDamping = groundDrag;
        // Movement
        SetMovementSpeed();
        MovePlayer();
        SpeedControl();
    }



    #endregion MonoBehavior


    #region Basic Movement

    public void StartMovement(float horizontalInput, float verticalInput)
    {
        this.horizontalInput = horizontalInput;
        this.verticalInput = verticalInput;
    }

    public void StopMovement()
    {
        accelerationIncrement = -math.abs(accelerationIncrement);
        horizontalInput = 0;
        verticalInput = 0;
    }
    public void OnSprint()
    {
        isSprinting = true;
    }
    public void OnSprintEnd()
    {
        isSprinting = false;
    }
    
    
    void SetMovementSpeed()
    {
        if (isSprinting)
        {
            moveSpeed = sprintSpeed;
        }
        else
        {
            moveSpeed = basicSpeed;
        }
    }

    void MovePlayer()
    {
        // moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection = Camera.forward * verticalInput + orientation.right * horizontalInput;
        MovementForce(rb.linearDamping);
    }

    void MovementForce(float multiplier)
    {
        // Multiplying the drag means it will only affect the player when they stop holding a movement button
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f * multiplier, ForceMode.Force);
    }

    // Makes sure the movement speed doesnt go over a certain amount
    void SpeedControl()
    {

        Vector3 curVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);

        if (curVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVel = curVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, limitedVel.y, limitedVel.z);
        }

    }
}

    #endregion Basic Movement Functions

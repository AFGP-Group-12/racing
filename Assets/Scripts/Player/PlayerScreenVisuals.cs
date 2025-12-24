using UnityEngine;
using System;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerScreenVisuals : MonoBehaviour
{

    [Header("Speed Lines")]

    [SerializeField] GameObject speedLineGameObject;
    [SerializeField] Camera playerCamera;
    [SerializeField] float baseAddedFov; // How much fov do you want to be added to the field of view
    private RawImage speedLineRawImage;

    [Header("Rotation on Move")]

    [SerializeField] PlayerCamera playerCameraScript;
    [SerializeField] float rotationIncrement;
    [SerializeField] float setRotationAdditive;

    private float targetRotation;
    private float curRotation = 0;

    private float fovSmoothTime = 0.15f; 
    private float startingFov;
    private float transparency;
    private float currentSprintFov;
    private float targetFOV;
    private float fovVel; // velocity tracker for SmoothDamp


    [Header("Screen Shake")]
    private AnimationCurve shakeCurve;
    private float shakeDuration;
    private float shakeElapsedTime;

    [Header("Fov Change")]
    private AnimationCurve fovCurve;
    private float fovDuration;
    private float fovElapsedTime;
    private float extraFov;

    [Header("Wall Running effect")]
    private float wallFovChange;

    [Header("Timer")]
    public WindowTimer windowTimer;

    private PlayerContext context;
    private PlayerMovement movement;
    private PlayerStateHandler stateHandler;
    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        context = GetComponent<PlayerContext>();
        movement = context.movement;
        rb = context.rb;
        stateHandler = context.stateHandler;

        rb.freezeRotation = true;

        startingFov = playerCamera.fieldOfView;

        speedLineRawImage = speedLineGameObject.GetComponent<RawImage>();

    }
    void FixedUpdate()
    {
        wallFovChange = movement.GetWallRunFovChange();
        ScreenShakeCycle();
        CycleAddFOV();
    }

    public void SetSpeedVisuals(float basicSpeed, float maxSpeed, float moveSpeed, MovementState state)
    {

        basicSpeed++;
        float speedDifference = maxSpeed - basicSpeed;
        float moveDifference = moveSpeed - basicSpeed;

        transparency = moveDifference / speedDifference;

        transparency = Mathf.Clamp(transparency, 0, 100);

        speedLineRawImage.material.SetFloat("_Transparency", transparency);

        currentSprintFov = transparency;
        targetFOV = startingFov + (baseAddedFov * currentSprintFov);

        targetFOV = Mathf.Lerp(startingFov, startingFov + baseAddedFov, transparency);
        playerCamera.fieldOfView = Mathf.SmoothDamp(playerCamera.fieldOfView, targetFOV, ref fovVel, fovSmoothTime) + extraFov;
    }


    // ****Might come back here and clean it up a bit. I feel like it could be more efficient****
    public void MoveRotation(float horizontalInput, bool customAdditive, float customRotationAdditive)
    {

        float rotationAdditive = customAdditive ? customRotationAdditive : setRotationAdditive;

        if (math.abs(targetRotation) > math.abs(rotationAdditive))
        {
            targetRotation = rotationAdditive;
        }

        if (horizontalInput != 0)
        {
            float horizontalInputDirection = -horizontalInput / math.abs(horizontalInput);
            targetRotation = rotationAdditive * horizontalInputDirection;
            rotationIncrement = math.abs(rotationIncrement) * horizontalInputDirection;
            curRotation = playerCameraScript.getRotationZ();

            if (targetRotation < 0 && curRotation > targetRotation || targetRotation > 0 && curRotation < targetRotation)
            {
                curRotation += rotationIncrement;
            }

            playerCameraScript.setRotationZ(curRotation);
        }
        else
        {
            targetRotation = 0;
            rotationIncrement = math.abs(rotationIncrement);
            curRotation = playerCameraScript.getRotationZ();

            if (curRotation > targetRotation)
            {
                curRotation -= rotationIncrement;
            }
            else if (curRotation < targetRotation)
            {
                curRotation += rotationIncrement;
            }

            playerCameraScript.setRotationZ(curRotation);
        }

    }
    

    public void ScreenShake(AnimationCurve strengthCurve, float duration )
    {
        shakeCurve = strengthCurve;
        shakeDuration = duration;
        shakeElapsedTime = 0;
    }

    private void ScreenShakeCycle()
    {
        if (shakeDuration > shakeElapsedTime)
        {
            shakeElapsedTime += Time.deltaTime;
            playerCameraScript.SetShake(shakeCurve, shakeDuration, shakeElapsedTime);
        }
        else
        {
            shakeElapsedTime = 0;
            shakeDuration = 0;
        }

    }
    public void StopScreenShake()
    {
        playerCameraScript.StopShake();
    }

    public void StartAddFOV(AnimationCurve fovCurve, float duration)
    {
        this.fovCurve = fovCurve;
        fovDuration = duration;
        fovElapsedTime = 0;
    }
    
    private void CycleAddFOV()
    {
        if (stateHandler.state == MovementState.wallrunningleft || stateHandler.state == MovementState.wallrunningright)
        {
            fovElapsedTime += 0.1f;
            fovElapsedTime = Mathf.Clamp(fovElapsedTime, 0f, 1f);


            extraFov = Mathf.Lerp(0, wallFovChange, fovElapsedTime);
        }
        else if (fovDuration > fovElapsedTime)
        {
            fovElapsedTime += Time.deltaTime;
            extraFov = fovCurve.Evaluate(fovElapsedTime / fovDuration);
        }
        else
        {
            fovElapsedTime = 0;
            fovDuration = 0;
            extraFov = 0;
        }

    }
    public void StopAddFOV()
    {
        playerCameraScript.StopShake();
    }

    // Used for debugging
    void SetRotation()
    {
        playerCameraScript.setRotationZ(curRotation);
    }



    
}

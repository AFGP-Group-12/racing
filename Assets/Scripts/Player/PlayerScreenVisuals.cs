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
    [SerializeField] float addedFov; // How much fov do you want to be added to the field of view

    private float fovSmoothTime = 0.15f; 
    private RawImage speedLineRawImage;

    [Header("Rotation on Move")]

    [SerializeField] PlayerCamera playerCameraScript;
    [SerializeField] float rotationIncrement;
    [SerializeField] float setRotationAdditive;

    private float targetRotation;
    private float curRotation = 0;
    private float startingFov;
    private float transparency;
    private float currentAddedFov;
    private float targetFOV;
    private float fovVel; // velocity tracker for SmoothDamp


    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

        startingFov = playerCamera.fieldOfView;

        speedLineRawImage = speedLineGameObject.GetComponent<RawImage>();

    }
    void Update()
    {
    }

    public void SetSpeedVisuals(float basicSpeed, float maxSpeed, float moveSpeed, MovementState state)
    {

        basicSpeed++;
        float speedDifference = maxSpeed - basicSpeed;
        float moveDifference = moveSpeed - basicSpeed;

        transparency = moveDifference / speedDifference;

        if (transparency < 0)
        {
            transparency = 0;
        }

        if (transparency > 100)
        {
            transparency = 100;
        }

        speedLineRawImage.material.SetFloat("_Transparency", transparency);

        currentAddedFov = transparency;
        targetFOV = startingFov + (addedFov * currentAddedFov);

        targetFOV = Mathf.Lerp(startingFov, startingFov + addedFov, transparency);
        playerCamera.fieldOfView = Mathf.SmoothDamp(playerCamera.fieldOfView, targetFOV, ref fovVel, fovSmoothTime);
    }


    // ****Might come back here and clean it up a bit. I feel like it could be more efficient****
    public void MoveRotation(float horizontalInput, bool customAdditive,float customRotationAdditive)
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

    // Used for debugging
    void SetRotation()
    {
        playerCameraScript.setRotationZ(curRotation);
    }
}

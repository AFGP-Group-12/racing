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

    private RawImage speedLineRawImage;

    [Header("Rotation on Move")]

    [SerializeField] PlayerCamera playerCameraScript;

    [SerializeField] float rotationIncrement;

    [SerializeField] float rotationAdditive;

    private float targetRotation;

    private float curRotation = 0;


    private float startingFov;

    private float transparency;

    private float currentAddedFov;

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

        startingFov = playerCamera.fieldOfView;

        speedLineRawImage = speedLineGameObject.GetComponent<RawImage>();

    }

    public void SetSpeedVisuals(float basicSpeed, float maxSpeed, float moveSpeed)
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
        playerCamera.fieldOfView = startingFov + (addedFov * currentAddedFov);
    }


    // ****Might come back here and clean it up a bit. I feel like it could be more efficient****
    public void MoveRotation(float horizontalInput)
    {
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

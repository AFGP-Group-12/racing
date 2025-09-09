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

    public void SetSpeedVisuals(float basicSpeed, float sprintSpeed, float moveSpeed)
    {
        float speedDifference = sprintSpeed - basicSpeed;
        float moveDifference = moveSpeed - basicSpeed;

        transparency = moveDifference / speedDifference;

        if (transparency < 0)
        {
            transparency = 0;
        }

        speedLineRawImage.material.SetFloat("_Transparency", transparency);

        currentAddedFov = transparency;
        playerCamera.fieldOfView = startingFov + (addedFov * currentAddedFov);
    }

    public void MoveRotation(float horizontalInput)
    {
        if (math.abs(targetRotation) > math.abs(rotationAdditive))
        {
            targetRotation = rotationAdditive;
        }

        if (horizontalInput != 0)
        {
            targetRotation = rotationAdditive * (-horizontalInput / math.abs(horizontalInput));
            rotationIncrement = math.abs(rotationIncrement) * (-horizontalInput / math.abs(horizontalInput));
            curRotation = playerCameraScript.getRotationZ();

            if (curRotation != targetRotation)
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

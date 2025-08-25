using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    [SerializeField] Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void MovePlayer()
    {
        
    }

    void forwardHeld()
    {
        horizontalInput = 1;
    }
    void forwardReleased()
    {
        horizontalInput = 0;
    }

    void backwardsHeld()
    {
        horizontalInput = -1;
    }
    void backwardsReleased()
    {
        horizontalInput = 0;
    }

    void rightHeld()
    {
        verticalInput = -1;
    }
    void rightReleased()
    {
        verticalInput = 0;
    }

    void leftHeld()
    {
        verticalInput = -1;
    }
    void leftReleased()
    {
        verticalInput = 0;
    }
}

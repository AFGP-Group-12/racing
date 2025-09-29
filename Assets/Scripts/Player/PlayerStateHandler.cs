using UnityEngine;

public class PlayerStateHandler : MonoBehaviour
{
    [Header("State Machine")]

    public MovementState state{ get; private set;}

    public bool isSprinting { get; set; }
    public bool isWallrunning { get; set; }
    public bool isSliding { get; set; }
    public bool isDashing { get; set; }
    public bool isOnGround { get; set; }

    #region State Handler
    void Update()
    {
        if (isWallrunning)
        {
            state = MovementState.wallrunning;
        }
        else if (isDashing)
        {
            state = MovementState.dashing;
        }
        else if (!isOnGround)
        {
            state = MovementState.air;
        }
        else if (isSliding)
        {
            state = MovementState.sliding;
        }
        else if (isOnGround && isSprinting)
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
    


    
}

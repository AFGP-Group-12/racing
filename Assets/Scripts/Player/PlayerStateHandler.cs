using System;
using UnityEngine;

public class PlayerStateHandler : MonoBehaviour
{
    [Header("State Machine")]

    public MovementState state { get; private set; }
    private MovementState prevState{ get; set; }

    public bool isSprinting { get; set; }
    public bool isWallrunningLeft { get; set; }
    public bool isWallrunningRight { get; set; }
    public bool isSliding { get; set; }
    public bool isDashing { get; set; }
    public bool isGrappling{ get; set; }
    public bool isOnGround { get; set; }
    public bool isWalking { get; set; }

    public event Action<MovementState> OnStateChanged;

    #region State Handler

    void Awake()
    {
        state = MovementState.idle;
        prevState = MovementState.idle;
    }

    void Update()
    {
        state = true switch
        {
            var _ when isGrappling => MovementState.grappling,
            var _ when isWallrunningLeft => MovementState.wallrunningleft,
            var _ when isWallrunningRight => MovementState.wallrunningright,
            var _ when isDashing => MovementState.dashing,
            var _ when !isOnGround => MovementState.air,
            var _ when isSliding => MovementState.sliding,
            var _ when isOnGround && isSprinting => MovementState.sprinting,
            var _ when isOnGround && isWalking && !isSprinting => MovementState.walking,
            _ => MovementState.idle,
        };

        if (state != prevState)
        {
            OnStateChanged?.Invoke(state);
            prevState = state;
            if (GameplayClient.instance != null) { GameplayClient.instance.CurrentState = state; }
        }
    }

    void SpeedCheck()
    {
        // currentSpeed = rb.linearVelocity.magnitude;
    }

    #endregion State Handler
    


    
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerContext : MonoBehaviour
{
    public Rigidbody rb { get; private set; }
    public PlayerInput input { get; private set; }
    public PlayerInputHandler inputHandler { get; private set; }
    public PlayerStateHandler stateHandler { get; private set; }
    public PlayerMovement movement { get; private set; }
    public PlayerScreenVisuals screenVisuals { get; private set; }
    public PlayerAbilityManager abilityManager { get; private set; }

    public Vector3 playerAim { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        inputHandler = GetComponent<PlayerInputHandler>();
        stateHandler = GetComponent<PlayerStateHandler>();
        movement = GetComponent<PlayerMovement>();
        screenVisuals = GetComponent<PlayerScreenVisuals>();
        abilityManager = GetComponent<PlayerAbilityManager>();
    }
}

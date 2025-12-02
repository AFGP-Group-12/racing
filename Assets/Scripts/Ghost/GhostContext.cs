using UnityEngine;
using UnityEngine.InputSystem;


public class GhostContext : MonoBehaviour
{
    public Rigidbody rb { get; private set; }
    public PlayerInput input { get; private set; }
    public GhostInputHandler inputHandler { get; private set; }
    public GhostMovement movement { get; private set; }  
    public GameObject playerObject;
    public Transform orientation;
    public Transform cameraTransform { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        inputHandler = GetComponent<GhostInputHandler>();
        movement = GetComponent<GhostMovement>();
        cameraTransform = Camera.main.transform; // Change this to a regular public if something goes wrong here
    }
}

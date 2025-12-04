using UnityEngine;
using UnityEngine.InputSystem;

public class DebuggingScript : MonoBehaviour
{

    private PlayerInput input;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = GetComponent<PlayerInput>();

        input.actions["DebugButton"].performed += PlaceNode;
    }
    

    void PlaceNode(InputAction.CallbackContext context)
    {
        FoutchTools.SetPlayerNode();
    }
}

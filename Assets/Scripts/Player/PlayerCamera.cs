using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivityX;
    public float sensitivityY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    [SerializeField] InputActionReference mouse;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Cursor is locked to the middle of the screen. Use unlock cursor function to unlock it.
        lockCursor();
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = mouse.action.ReadValue<Vector2>().x * Time.deltaTime * sensitivityX;
        float mouseY = mouse.action.ReadValue<Vector2>().y * Time.deltaTime * sensitivityY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);


        // rotate the camera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void lockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void unlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

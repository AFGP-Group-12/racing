using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float sensitivityX;
    [SerializeField] float sensitivityY;

    [SerializeField] Transform orientation;

    private float xRotation;
    private float yRotation;
    private float zRotation;

    [SerializeField] InputActionReference mouse;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Cursor is locked to the middle of the screen. Use unlock cursor function to unlock it.
        LockCursor();
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
        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void setRotationZ(float zRotation)
    {
        this.zRotation = zRotation;
    }
    public float getRotationZ()
    {
        return zRotation;
    }
}

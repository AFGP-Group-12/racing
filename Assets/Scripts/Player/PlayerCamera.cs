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

    private Vector3 shakeAmount;

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
        transform.rotation = Quaternion.Euler(xRotation + shakeAmount.x, yRotation + shakeAmount.y, zRotation + shakeAmount.z);
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

    public void SetShake(float strength, float strengthVelocity, float smoothTime)
    {
        float randomX = Random.value - 0.5f;
        float randomY = Random.value - 0.5f;
        float randomZ = Random.value - 0.5f;

        // strength = Mathf.SmoothDamp(strength, 0f, ref strengthVelocity, smoothTime);

        Debug.Log("??");
        shakeAmount = new Vector3(randomX, randomY, randomZ) * strength;
    }

    public void StopShake()
    {
        shakeAmount = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.Euler(xRotation , yRotation , zRotation );
    }
    
}

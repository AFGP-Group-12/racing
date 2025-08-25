using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;

    // Update is called once per frame
    void Update()
    {
        transform.position = cameraTransform.position;
    }
}

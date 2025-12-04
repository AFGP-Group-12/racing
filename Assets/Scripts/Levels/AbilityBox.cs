using UnityEngine;

class AbilityBox : MonoBehaviour
{
    [Header("Billboarding")]
    [Tooltip("Camera the sprite should face. Defaults to Main Camera if null.")]
    [SerializeField] private Camera targetCamera;
    [Tooltip("If true, only rotate around Y (upright billboarding). If false, fully face camera.")]
    [SerializeField] private bool constrainToYAxis = true;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 cameraPos = targetCamera.transform.position;
        Vector3 lookTarget = cameraPos;

        if (constrainToYAxis)
        {
            // Keep sprite upright: rotate only around Y to face camera horizontally
            lookTarget = new Vector3(cameraPos.x, transform.position.y, cameraPos.z);
            Vector3 direction = (lookTarget - transform.position);
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = targetRotation;
            }
        }
        else
        {
            // Fully face the camera (classic billboard)
            Vector3 direction = (cameraPos - transform.position);
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = targetRotation;
            }
        }
    }

}
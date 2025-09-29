using UnityEngine;

public class PreviewPlatform : MonoBehaviour
{
    public void MoveTransform(Vector3 newTransform)
    {
        gameObject.transform.position = newTransform;
    }
    public void DisableObject()
    {
        gameObject.SetActive(false);
    }
    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}

using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<DeathboxHandler>().HandleCheckpointReached(respawnPoint);
        }
    }
}

using System;
using UnityEngine;

public class Deathbox : MonoBehaviour
{
    [SerializeField]
    private Vector3 respawnPoint;
    [SerializeField]
    private int levelIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<DeathboxHandler>()?.HandlePlayerDeath(levelIndex);
            other.transform.position = respawnPoint;
        }
    }
}

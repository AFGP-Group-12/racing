using System;
using UnityEngine;

public class Deathbox : MonoBehaviour
{
    [SerializeField]
    private int levelIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // other.GetComponentInParent<DeathboxHandler>()?.HandlePlayerDeath(levelIndex);
            other.GetComponentInParent<DeathboxHandler>()?.HandlePlayerDeath(levelIndex);
        }
    }
}

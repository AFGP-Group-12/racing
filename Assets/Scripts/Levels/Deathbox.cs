using System;
using UnityEngine;

public class Deathbox : MonoBehaviour
{
    [SerializeField]
    private Vector3 respawnPoint;
    [SerializeField]
    private int levelIndex;

    public event Action<int> OnPlayerDeath;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = respawnPoint;
            OnPlayerDeath?.Invoke(levelIndex);
        }
    }
}

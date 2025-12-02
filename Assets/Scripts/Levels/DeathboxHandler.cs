using System;
using Messages;
using UnityEngine;
using UnityEngine.Animations;

public class DeathboxHandler : MonoBehaviour
{
    [SerializeField]
    private Transform respawnPoint;

    public event Action<int> OnPlayerDeath;

    public event Action<int> OnLevelComplete;

    public void HandlePlayerDeath(int levelIndex)
    {
        // OnPlayerDeath?.Invoke(levelIndex);
        transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
    }

    public void HandleLevelComplete(int levelIndex)
    {
        OnLevelComplete?.Invoke(levelIndex);
        Debug.Log($"Level {levelIndex} completed");
    }

    public void HandleCheckpointReached(Transform checkpointTransform)
    {
        respawnPoint = checkpointTransform;
    }
}
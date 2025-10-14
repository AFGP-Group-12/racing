using System;
using Messages;
using UnityEngine;

public class DeathboxHandler : MonoBehaviour
{
    public event Action<int> OnPlayerDeath;

    public event Action<int> OnLevelComplete;

    public void HandlePlayerDeath(int levelIndex)
    {
        OnPlayerDeath?.Invoke(levelIndex);
        Debug.Log($"Player died in {levelIndex}");
    }

    public void HandleLevelComplete(int levelIndex)
    {
        OnLevelComplete?.Invoke(levelIndex);
        Debug.Log($"Level {levelIndex} completed");
    }
}
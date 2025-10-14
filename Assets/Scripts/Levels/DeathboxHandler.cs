using System;
using Messages;
using UnityEngine;

public class DeathboxHandler : MonoBehaviour
{
    public event Action<int> OnPlayerDeath;

    public void HandlePlayerDeath(int levelIndex)
    {
        OnPlayerDeath?.Invoke(levelIndex);
        Debug.Log($"Player died in {levelIndex}");
    }
}
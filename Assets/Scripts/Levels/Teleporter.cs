using System;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField]
    private int currentLevel;

    [SerializeField]
    private Vector3 nextPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent.position = nextPosition;
            other.GetComponentInParent<DeathboxHandler>()?.HandleLevelComplete(currentLevel);
            other.GetComponentInParent<PlayerAbilityManager>()?.ResetAbilities();
        }
    }
}

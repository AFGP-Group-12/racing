using System;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField]
    private int currentLevel;
    [SerializeField]
    private int nextLevel;

    [SerializeField]
    private Vector3 nextPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
        }
    }
}

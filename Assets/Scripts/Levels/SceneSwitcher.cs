using System;
using UnityEngine;
using UnityEditor;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField]
    private string nextScene;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            Debug.Log("Exiting Play Mode in Editor");
#else
            Application.Quit();
            Debug.Log("Game Exited");
#endif
        }
    }
}

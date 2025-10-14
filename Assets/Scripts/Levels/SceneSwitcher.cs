using System;
using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField]
    private string nextScene;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }
}

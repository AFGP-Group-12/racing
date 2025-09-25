using Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Change : MonoBehaviour
{
    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.


        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MovementScene");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log("Scene Loaded");

        GameObject player = GameObject.Find("Player");

        player.SetActive(false);

        yield return new WaitForSeconds(2);

        player.SetActive(true);

    }

}

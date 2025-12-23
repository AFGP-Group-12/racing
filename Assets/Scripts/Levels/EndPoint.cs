using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class EndPoint : MonoBehaviour
{

    public bool moveElevator = false;
    [SerializeField] GameObject elavatorRoof;

    [SerializeField] Transform elevatorTargetPosition;
    [SerializeField] float elevatorMoveSpeed = 2f;

    [SerializeField] String currentSceneName;

    [SerializeField] String nextSceneName;

    private bool sceneUnloaded = false;

    private bool sceneLoaded = false;

    private float floorRoofDifference;
    void Start()
    {
        floorRoofDifference = elavatorRoof.transform.position.y - gameObject.transform.position.y;
    }
    void Update()
    {
        if (moveElevator)
        {
            MoveElevator();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Player reached endpoint");
            // GameplayClient.instance.PlayerReachedEndPoint();


            // Debugging additive loading
            moveElevator = true;
        }
        else if(other.CompareTag("UnloadPoint") && !sceneUnloaded)
        {
            Debug.Log("Scene Unloading");
            sceneUnloaded = true;
            StartCoroutine(UnLoadYourAsyncScene());
        }
    }

    void MoveElevator()
    {
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, elevatorTargetPosition.position, elevatorMoveSpeed * Time.deltaTime);
        elavatorRoof.transform.position = Vector3.MoveTowards(elavatorRoof.transform.position, elevatorTargetPosition.position + Vector3.up * floorRoofDifference, elevatorMoveSpeed * Time.deltaTime);
    }
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {

        }
    }

    IEnumerator UnLoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(currentSceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return StartCoroutine(LoadYourAsyncScene());
        }
    }
    IEnumerator LoadYourAsyncScene()
    {
        Debug.Log("Scene loading");
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            sceneLoaded = true;
            yield return null;
        }
    }
}

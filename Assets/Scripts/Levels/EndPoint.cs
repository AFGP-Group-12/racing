using UnityEngine;

public class EndPoint : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
       
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Player reached endpoint");
            GameplayClient.instance.PlayerReachedEndPoint();
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {

        }
    }
}

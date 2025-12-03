using UnityEngine;

public class EndPoint : MonoBehaviour
{
    float playersInEndpoint = 0;

    public float currentPlayers = 4;
    float requiredPlayers; // This has to change depending on the total players in the game
    void Start()
    {
        requiredPlayers = currentPlayers - 1;
    }
    void Update()
    {
        if(playersInEndpoint >= requiredPlayers)
        {
            Debug.Log("Level Complete!");

            // Then turn one of the players into a ghost in the next level

            // Transition to next level
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playersInEndpoint += 1;
            Debug.Log("End Point Reached by: " + other.name + " Total Players: " + playersInEndpoint);

        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playersInEndpoint -= 1;
            Debug.Log("End Point Left by: " + other.name + " Total Players: " + playersInEndpoint);

        }
    }
}

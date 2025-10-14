using UnityEngine;

public class Logs : MonoBehaviour
{
    string filePath;
    int deathCounter = 0;
    private Deathbox deathOccured;

    private void Start()
    {
        deathOccured = FindFirstObjectByType<Deathbox>();
        filePath = Application.dataPath + "/logs.txt";
        deathOccured.OnPlayerDeath += IncrementDeathCounter;
    }

    private void IncrementDeathCounter(int levelIndex)
    {
        deathCounter++;
        appendLog();
    }

    public void appendLog()
    {
        System.IO.File.AppendAllText(filePath, "Death!" + "\n");
        System.IO.File.AppendAllText(filePath, deathCounter + "\n");
    }

    void OnDestroy()
    {
        if (deathOccured != null)
        {
            deathOccured.OnPlayerDeath -= IncrementDeathCounter; 
        }
    }

}

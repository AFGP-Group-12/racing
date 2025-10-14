using UnityEngine;
using System.Diagnostics;

public class Logs : MonoBehaviour
{
    private string filePath;
    private int[] deathCounter = new int[5];
    private DeathboxHandler deathOccured;
    private Stopwatch stopwatch;

    private void Start()
    {
        stopwatch = new Stopwatch();
        stopwatch.Start();
        deathOccured = GetComponent<DeathboxHandler>();
        filePath = Application.dataPath + "/logs.txt";
        deathOccured.OnPlayerDeath += IncrementDeathCounter;
        deathOccured.OnLevelComplete += SendLevelLog;
    }

    private void IncrementDeathCounter(int levelIndex)
    {
        deathCounter[levelIndex-1]++ ;
    }
    private void SendLevelLog(int levelIndex)
    {
        stopwatch.Stop();
        System.IO.File.AppendAllText(filePath, "Level" + levelIndex + ": " + deathCounter[levelIndex-1] + ": " + stopwatch.Elapsed.TotalSeconds + "\n");
        stopwatch.Reset();
    }

    void OnDestroy()
    {
        if (deathOccured != null)
        {
            deathOccured.OnPlayerDeath -= IncrementDeathCounter; 
        }
        if(deathOccured != null)
        {
            deathOccured.OnLevelComplete -= SendLevelLog;
        }
    }

}

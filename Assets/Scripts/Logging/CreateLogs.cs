using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class Logs : MonoBehaviour
{
    private string logPath;
    private int[] deathCounter = new int[6];
    private DeathboxHandler deathOccured;
    private Stopwatch stopwatch;

    private void Start()
    {
        stopwatch = new Stopwatch();
        stopwatch.Start();
        deathOccured = GetComponent<DeathboxHandler>();
        logPath = Application.dataPath + "/logs.txt";
        deathOccured.OnPlayerDeath += IncrementDeathCounter;
        deathOccured.OnLevelComplete += SendLevelLog;
        AssetDatabase.DeleteAsset(logPath);
    }

    private void IncrementDeathCounter(int levelIndex)
    {
        deathCounter[levelIndex-1]++ ;
    }
    private void SendLevelLog(int levelIndex)
    {
        UnityEngine.Debug.Log("Level " + levelIndex + " completed with " + deathCounter[levelIndex-1] + " deaths in " + stopwatch.Elapsed.TotalSeconds + " seconds.");
        stopwatch.Stop();
        System.IO.File.AppendAllText(logPath, "Level" + levelIndex + ": " + deathCounter[levelIndex-1] + ": " + stopwatch.Elapsed.TotalSeconds + "\n");
        stopwatch.Reset();
        stopwatch.Start();
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

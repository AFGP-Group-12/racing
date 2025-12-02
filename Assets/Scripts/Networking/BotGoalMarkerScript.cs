using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BotGoalMarkerScript : MonoBehaviour
{
    private static List<BotGoalMarkerScript> allMarkers = new List<BotGoalMarkerScript>();
    public int pathNumber { get; set; }

    public bool buttonDisplayName; //"run" or "generate" for example
    public bool buttonDisplayName2; //supports multiple buttons


    private void Awake()
    {
        allMarkers.Add(this);
    }

    private void OnValidate()
    {
        if (buttonDisplayName)
            ButtonFunction1 ();
        else if (buttonDisplayName2)
            ButtonFunction2 ();
        buttonDisplayName = false;
        buttonDisplayName2 = false;
    }

    void ButtonFunction1 ()
    {
        //DoStuff
    }

    void ButtonFunction2 ()
    {
        //DoStuff
    }

}

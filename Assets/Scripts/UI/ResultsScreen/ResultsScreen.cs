using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultsScreen : MonoBehaviour
{
    public List<String> playerNames;
    public List<GameObject> PlaceImages; // Only have 4 of them
    public TextMeshProUGUI PlaceNamesText;

    void Start()
    {
        for(int i = 0; i < PlaceImages.Count; i++)
        {
            PlaceImages[i].SetActive(false);
        }

        // Debugging purposes
        // for(int i = 0; i < playerNames.Count; i++)
        // {
        //     PlaceImages[i].SetActive(true);
        // }
        // PlaceNamesText.text = String.Join("\n \n \n", playerNames);
    }

    public void AddNames(List<String> playerNames)
    {
        for(int i = 0; i < playerNames.Count; i++)
        {
            PlaceImages[i].SetActive(true);
           this.playerNames.Add(playerNames[i]);
        }
        PlaceNamesText.text = String.Join("\n \n \n", playerNames);
    }
}

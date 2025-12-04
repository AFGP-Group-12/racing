using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResultsScreen : MonoBehaviour
{
    public List<String> playerNames;
    public List<GameObject> PlaceImages; // Only have 4 of them
    public TextMeshProUGUI PlaceNamesText;

    private PlayerInput input;

    bool canExit = false;

    public float bufferTimer = 2f;

    void Start()
    {
        for(int i = 0; i < PlaceImages.Count; i++)
        {
            PlaceImages[i].SetActive(false);
        }
        input = GetComponent<PlayerInput>();
        input.actions["Any"].performed += buttonPressed;

        // Debugging purposes
        // for(int i = 0; i < playerNames.Count; i++)
        // {
        //     PlaceImages[i].SetActive(true);
        // }
        // PlaceNamesText.text = String.Join("\n \n \n", playerNames);
    }

    void Update()
    {
        if(bufferTimer > 0)
        {
            bufferTimer -= Time.deltaTime;
        }
        else
        {
            canExit = true;
            bufferTimer = 0f;
        }
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
    void buttonPressed(InputAction.CallbackContext context)
    {
        // Exit the game
        #if UNITY_EDITOR
            if (canExit)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
        #else
            if (canExit)
            {
                Application.Quit();
            }
        #endif
    }
}

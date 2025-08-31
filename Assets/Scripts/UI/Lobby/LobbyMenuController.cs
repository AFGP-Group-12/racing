using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private Label playerNameLabel1;
    private Label playerNameLabel2;
    private Label playerNameLabel3;
    private Label playerNameLabel4;

    private Label playerPingLabel1;
    private Label playerPingLabel2;
    private Label playerPingLabel3;
    private Label playerPingLabel4;

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;

        playerNameLabel1 = root.Q<Label>("playerNameLabel1");
        playerNameLabel2 = root.Q<Label>("playerNameLabel2");
        playerNameLabel3 = root.Q<Label>("playerNameLabel3");
        playerNameLabel4 = root.Q<Label>("playerNameLabel4");

        playerPingLabel1 = root.Q<Label>("playerPingLabel1");
        playerPingLabel2 = root.Q<Label>("playerPingLabel2");
        playerPingLabel3 = root.Q<Label>("playerPingLabel3");
        playerPingLabel4 = root.Q<Label>("playerPingLabel4");
    }

    private void UpdatePlayerName(string newName, int playerIndex)
    {
        switch (playerIndex)
        {
            case 1:
                playerNameLabel1.text = newName;
                break;
            case 2:
                playerNameLabel2.text = newName;
                break;
            case 3:
                playerNameLabel3.text = newName;
                break;
            case 4:
                playerNameLabel4.text = newName;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerIndex), "Invalid player index");
        }
    }

    private void UpdatePlayerPing(int newPing, int playerIndex)
    {
        switch (playerIndex)
        {
            case 1:
                playerPingLabel1.text = newPing.ToString();
                break;
            case 2:
                playerPingLabel2.text = newPing.ToString();
                break;
            case 3:
                playerPingLabel3.text = newPing.ToString();
                break;
            case 4:
                playerPingLabel4.text = newPing.ToString();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerIndex), "Invalid player index");
        }
    }
}
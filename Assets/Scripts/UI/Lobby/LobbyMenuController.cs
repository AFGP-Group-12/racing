using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private Label[] playerNameLabel;
    private Label[] playerPingLabel;
    private Label playerCountLabel;

    private Button exitLobbyButton;
    private Button startGameButton;

    private int playerCount = 0;
    private Dictionary<string, int> index_by_username = new Dictionary<string, int>();

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;

        playerNameLabel = new Label[4];
        playerPingLabel = new Label[4];
        for (int i = 0; i < 4; i++)
        {
            playerNameLabel[i] = root.Q<Label>("playerNameLabel" + (i + 1));
            playerPingLabel[i] = root.Q<Label>("playerPingLabel" + (i + 1));

            playerNameLabel[i].AddToClassList("hidden");
            playerPingLabel[i].AddToClassList("hidden");
        }
        playerCountLabel = root.Q<Label>("playerCountLabel");

        exitLobbyButton = root.Q<Button>("exitLobbyButton");
        startGameButton = root.Q<Button>("startGameButton");

        // exitLobbyButton.clicked += () => LobbyClient.instance.LeaveLobby();
        // startGameButton.clicked += () => LobbyClient.instance.StartGame();
    }

    public void Start()
    {
        LobbyClient.instance.OnPlayerJoinLobby += OnPlayerJoin;
        LobbyClient.instance.OnPlayerLeaveLobby += OnPlayerLeave;
    }

    private void OnPlayerJoin(string username)
    {
        index_by_username[username] = playerCount;
        UpdatePlayerName(username, playerCount++);
        playerCountLabel.text = playerCount.ToString() + "/4";
    }
    private void OnPlayerLeave(string username)
    {
        int player_index = index_by_username[username];

        foreach (var pair in index_by_username)
        {
            if (pair.Value > player_index)
            {
                index_by_username[pair.Key] = pair.Value - 1;
                UpdatePlayerName(pair.Key, pair.Value);
            }
        }

        UpdatePlayerName("", --playerCount);
        UpdatePlayerPing(-1, playerCount);

        playerCountLabel.text = playerCount.ToString() + "/4";
    }

    public void UpdatePlayerName(string newName, int playerIndex)
    {
        if (playerIndex >= 4) throw new ArgumentOutOfRangeException(nameof(playerIndex), "Invalid player index");

        playerNameLabel[playerIndex].text = newName;
        if (newName.Length == 0) playerNameLabel[playerIndex].AddToClassList("hidden");
        else playerNameLabel[playerIndex].RemoveFromClassList("hidden");
    }

    public void UpdatePlayerPing(int newPing, int playerIndex)
    {
        if (playerIndex >= 4) throw new ArgumentOutOfRangeException(nameof(playerIndex), "Invalid player index");

        playerPingLabel[playerIndex].text = newPing.ToString();
        if (newPing == -1) playerPingLabel[playerIndex].AddToClassList("hidden");
        else playerPingLabel[playerIndex].RemoveFromClassList("hidden");
    }
}
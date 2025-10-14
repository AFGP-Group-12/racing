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
    private Label lobbyNameLabel;
    private Label joinCodeLabel;

    private Button exitLobbyButton;
    private Button startGameButton;

    private int playerCount = 0;
    private Dictionary<string, int> index_by_username = new Dictionary<string, int>();
    private string host = "";
    private bool isHost = false;

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;

        playerNameLabel = new Label[4];
        playerPingLabel = new Label[4];
        for (int i = 0; i < 4; i++)
        {
            playerNameLabel[i] = root.Q<Label>("playerNameLabel" + (i + 1));
            playerPingLabel[i] = root.Q<Label>("playerPingLabel" + (i + 1));
        }
        playerCountLabel = root.Q<Label>("playerCountLabel");
        lobbyNameLabel = root.Q<Label>("lobbyNameLabel");
        joinCodeLabel = root.Q<Label>("joinCodeLabel");

        exitLobbyButton = root.Q<Button>("exitLobbyButton");
        startGameButton = root.Q<Button>("startGameButton");

        ClearPlayers();
        UpdateStartGameButton();
    }

    public void Start()
    {

        startGameButton.clicked += LobbyClient.instance.StartGame;
        exitLobbyButton.clicked += LobbyClient.instance.LeaveLobby;
        exitLobbyButton.clicked += ClearPlayers;

        LobbyClient.instance.OnPlayerJoinLobby += OnPlayerJoin;
        LobbyClient.instance.OnPlayerLeaveLobby += OnPlayerLeave;
        LobbyClient.instance.SetPlayerPing += SetPlayerPing;
        LobbyClient.instance.SetLobbyCode += SetLobbyCode;
        LobbyClient.instance.UpdateHost += SetHost;
    }

    private void SetPlayerPing(int ping_ms, string username)
    {
        if (!index_by_username.ContainsKey(username)) { return; }
        int player_index = index_by_username[username];
        UpdatePlayerPing(ping_ms, player_index);
    }

    private void ClearPlayers()
    {
        for (int i = 0; i < 4; i++)
        {
            playerNameLabel[i].AddToClassList("hidden");
            playerPingLabel[i].AddToClassList("hidden");
        }
        index_by_username.Clear();
        playerCount = 0;
        SetHost("", false);
    }

    private void OnPlayerJoin(string username)
    {
        index_by_username[username] = playerCount;
        UpdatePlayerName(username, playerCount++);
        playerCountLabel.text = playerCount.ToString() + "/4";

        UpdateStartGameButton();
    }
    private void OnPlayerLeave(string username)
    {
        int player_index = index_by_username[username];
        List<string> toModify = new List<string>();

        foreach (var pair in index_by_username)
        {
            if (pair.Value > player_index)
            {
                toModify.Add(pair.Key);
            }
        }
        foreach (string key in toModify)
        {
            UpdatePlayerName(key, --index_by_username[key]);
        }

        UpdatePlayerName("", --playerCount);
        UpdatePlayerPing(-1, playerCount);

        playerCountLabel.text = playerCount.ToString() + "/4";
        index_by_username.Remove(username);

        UpdateStartGameButton();
    }

    public void UpdatePlayerName(string newName, int playerIndex)
    {
        if (playerIndex >= 4) throw new ArgumentOutOfRangeException(nameof(playerIndex), "Invalid player index");

        if (newName.Length == 0) playerNameLabel[playerIndex].AddToClassList("hidden");
        else playerNameLabel[playerIndex].RemoveFromClassList("hidden");

        if (host == newName)
            newName += " (host)";

        playerNameLabel[playerIndex].text = newName;
    }

    public void UpdatePlayerPing(int newPing, int playerIndex)
    {
        if (playerIndex >= 4) throw new ArgumentOutOfRangeException(nameof(playerIndex), "Invalid player index");

        playerPingLabel[playerIndex].text = newPing.ToString() + "ms";
        if (newPing == -1) playerPingLabel[playerIndex].AddToClassList("hidden");
        else playerPingLabel[playerIndex].RemoveFromClassList("hidden");
    }

    public void SetLobbyName(string lobbyName)
    {
        lobbyNameLabel.text = lobbyName;
    }

    public void SetLobbyCode(string lobbyCode)
    {
        joinCodeLabel.text = lobbyCode;
        if (lobbyCode == "") joinCodeLabel.AddToClassList("hidden");
        else joinCodeLabel.RemoveFromClassList("hidden");
    }

    public void SetHost(string newHost, bool isHost)
    {
        this.isHost = isHost;
        host = newHost;
        
        // Remove previous host artifacts.
        foreach (var pair in index_by_username)
            UpdatePlayerName(pair.Key, pair.Value);

        if (index_by_username.ContainsKey(host))
            UpdatePlayerName(host, index_by_username[host]);

        UpdateStartGameButton();
    }
    private void UpdateStartGameButton()
    {
        startGameButton.SetEnabled(isHost && playerCount > 1);
    }
}
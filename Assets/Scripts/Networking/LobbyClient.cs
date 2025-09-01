using Messages;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class LobbyClient : MonoBehaviour
{
    string player_username;

    private Dictionary<int, string> username_by_id = new Dictionary<int, string>();

    // Networking
    private string connection_server_ip = "68.205.103.143";
    BaseNetworkClient client;

    public static LobbyClient instance;

    public event Action OnLobbyJoined;

    public event Action<string> OnPlayerJoinLobby;
    public event Action<string> OnPlayerLeaveLobby;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        player_username = "Player_" + UnityEngine.Random.Range(0, 10000000);
        client = new BaseNetworkClient(connection_server_ip, 8080, 256, player_username);
        client.ConnectToServer(OnConnect);
    }

    public unsafe void CreateLobby()
    {
        if (!client.IsConnectedTcp()) { Debug.LogError("Tried to create lobby, but not connected to server"); return; }

        racing_lobby_action_m message;
        message.type = (ushort)message_t.racing_lobby_action;
        message.action = 0;
        client.SendDataTcp(message.bytes, racing_lobby_action_m.size);
    }

    public unsafe void JoinLobby()
    {
        if (!client.IsConnectedTcp()) { Debug.LogError("Tried to join lobby, but not connected to server"); return; }

        racing_lobby_action_m message;
        message.type = (ushort)message_t.racing_lobby_action;
        message.action = 1;
        client.SendDataTcp(message.bytes, racing_lobby_action_m.size);
    }

    public unsafe void UpdateUsername(string new_username)
    {
        player_username = new_username;
        // Send an update username message.

        racing_lobby_action_m message;
        message.type = (ushort)message_t.racing_lobby_action;
        message.action = 4;
        Helpers.fillUsername(new_username, out message.username);
        client.SendDataTcp(message.bytes, racing_lobby_action_m.size);
    }
    private void OnConnect()
    {
        Debug.Log("LobbyClient Connected!");
    }

    private void HandleLobbyUpdate(racing_lobby_update_m message) {
        string other_player_username;
        switch (message.update)
        {
            case 0: // Lobby joined sucessfully
                OnLobbyJoined.Invoke();
                OnPlayerJoinLobby.Invoke(player_username);
                break;
            case 1: // Other player joined lobby
                other_player_username = Helpers.readUsername(message.username);
                username_by_id[message.other_player_id] = other_player_username;
                OnPlayerJoinLobby.Invoke(other_player_username);
                break;
            case 2: // Other player left lobby
                other_player_username = username_by_id[message.other_player_id];
                OnPlayerLeaveLobby.Invoke(other_player_username);
                break;
            case 3: // Invalid lobby code
                Debug.LogError("Invalid Lobby Code!");
                break;
            case 4: // Ping
                break;
        }
    }

    private void Update()
    {
        generic_m message;
        while (client.TryDequeueMessage(out message))
        {
            switch(message.get_t())
            {
                case message_t.connection_reply:
                    break;
                case message_t.racing_lobby_update:
                    HandleLobbyUpdate(message.racing_lobby_update);
                    break;
                default:
                    break;
            }
        }
    }
    private void OnDestroy()
    {
        client.Disconnect();
    }
}

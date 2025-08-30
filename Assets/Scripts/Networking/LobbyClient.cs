using Messages;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.MemoryProfiler;
using UnityEngine;

struct PlayerLobbyData
{
    string name;
    int champion;
    bool connected;
}

public class LobbyClient : MonoBehaviour
{
    // Lobby State
    private List<PlayerLobbyData> players;
    private string lobby_code = "";
    bool isHost = false;
    int selected_map;
    string player_username;

    // Networking
    private string connection_server_ip = "68.205.103.143";
    BaseNetworkClient client;

    public static LobbyClient instance;

    void Start()
    {
        instance = this;
        player_username = "a_user" + UnityEngine.Random.Range(0, 10000000);
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        players = new List<PlayerLobbyData>();
        client = new BaseNetworkClient(connection_server_ip, 8080, 256, player_username);
        client.ConnectToServer(OnConnect);
    }

    public void CreateLobby()
    {
        if (!client.IsConnectedTcp()) { Debug.LogError("Tried to create lobby, but not connected to server"); return; }


    }

    public void JoinLobby()
    {
        if (!client.IsConnectedTcp()) { Debug.LogError("Tried to join lobby, but not connected to server"); return; }


    }

    private void OnConnect()
    {
        Debug.Log("LobbyClient Connected!");
    }

    private void Update()
    {
        generic_m message;
        while (client.TryDequeueMessage(out message))
        {
            switch(message.get_t())
            {
                case message_t.connection_reply:
                    Debug.Log("You are: " + message.connection_reply.id);
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

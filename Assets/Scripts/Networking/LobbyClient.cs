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

    // Networking
    private string connection_server_ip = "68.205.103.143";
    BaseNetworkClient client;

    void Start()
    {
        players = new List<PlayerLobbyData>();

        client = new BaseNetworkClient(connection_server_ip, 8080, 256, "a_username");//UserDataScript.username);
        client.ConnectToServer(OnConnect);
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
                case message_t.connection:
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

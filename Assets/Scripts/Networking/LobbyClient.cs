using Messages;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyClient : MonoBehaviour
{
    string player_username;

    private Dictionary<int, string> username_by_id = new Dictionary<int, string>();

    // Networking
    private bool is_local = false;
    private string connection_server_ip = "69.62.71.12";
    private string local_connection_server_ip = "192.168.0.201";
    public BaseNetworkClient client;

    public static LobbyClient instance;

    public event Action OnLobbyJoined;
    public event Action OnLobbyExited;

    public event Action<string> OnPlayerJoinLobby;
    public event Action<string> OnPlayerLeaveLobby;

    public event Action<int, string> SetPlayerPing;
    public event Action<string> SetLobbyCode;
    public event Action<string, bool> UpdateHost;

    private System.Diagnostics.Stopwatch pingTimer;
    private bool waitingForPing = false;
    DateTime timeSinceLastPing;
    int currentPing = -1;
    const int pingInterval = 5;
    int self_id = -1;

    bool gameStarted = false;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);


        pingTimer = new System.Diagnostics.Stopwatch();
        timeSinceLastPing = DateTime.Now;
    }

    void Start()
    {
        player_username = "Player_" + UnityEngine.Random.Range(0, 10000000);
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        Debug.Log("Connecting to server...");
        client = new BaseNetworkClient(is_local ? local_connection_server_ip : connection_server_ip, 8080, 256, player_username);
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
    public unsafe void JoinPrivateLobby(string lobby_code)
    {
        racing_lobby_action_m message;
        message.type = (ushort)message_t.racing_lobby_action;
        message.action = 2;
        Helpers.setStringForMessage(lobby_code, 6, message.lobby_code);
        client.SendDataTcp(message.bytes, racing_lobby_action_m.size);
    }

    public unsafe void UpdateUsername(string new_username)
    {
        player_username = new_username;
        // Send an update username message.

        client.changeUsername(player_username);

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

    private unsafe void HandleLobbyUpdate(racing_lobby_update_m message)
    {
        string other_player_username;
        switch (message.update)
        {
            case 0: // Public lobby joined sucessfully
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
                OnLobbyExited.Invoke();
                break;
            case 4: // Private lobby created
                OnLobbyJoined.Invoke();
                OnPlayerJoinLobby.Invoke(player_username);
                string lobby_code = Helpers.getStringFromMessage(message.lobby_code, 6);
                SetLobbyCode.Invoke(lobby_code);
                break;
            case 5: // Private lobby Full
                Debug.LogError("Lobby full!");
                break;
            case 6: // Private lobby joined
                OnLobbyJoined.Invoke();
                OnPlayerJoinLobby.Invoke(player_username);
                break;
            case 7: // Lobby exited sucessfully
                OnLobbyExited.Invoke();
                Debug.Log("Exited");
                break;
            case 8: // Other player ping
                other_player_username = username_by_id[message.other_player_id];
                SetPlayerPing.Invoke(message.ping, other_player_username);
                break;
            case 9: // Host changed
                other_player_username = username_by_id[message.other_player_id];
                bool is_host = message.other_player_id == self_id;
                UpdateHost.Invoke(other_player_username, is_host);
                break;

        }
    }

    private void Update()
    {
        generic_m message;
        while (client.TryDequeueMessage(out message))
        {
            switch (message.get_t())
            {
                case message_t.connection_reply:
                    self_id = message.connection_reply.id;
                    break;
                case message_t.racing_lobby_update:
                    HandleLobbyUpdate(message.racing_lobby_update);
                    break;
                case message_t.ping_reply:
                    HandlePing();
                    break;
                default:
                    Debug.Log("Got Unexpected: " + message.get_t());
                    break;
            }
        }
    }

    private unsafe void HandlePing()
    {
        if (!waitingForPing) { Debug.LogError("Got ping when not expecting it!"); return; }

        pingTimer.Stop();
        currentPing = (int)pingTimer.ElapsedMilliseconds;
        SetPlayerPing.Invoke(currentPing, player_username);

        racing_lobby_action_m message;
        message.type = (ushort)message_t.racing_lobby_action;
        message.action = 6;
        message.ping = (ushort)currentPing;
        client.SendDataTcp(message.bytes, racing_lobby_action_m.size);


        waitingForPing = false;
        timeSinceLastPing = DateTime.Now;
        pingTimer.Reset();
    }

    private unsafe void FixedUpdate()
    {
        if (waitingForPing) { return; }

        Double elapsedseconds = (DateTime.Now - timeSinceLastPing).TotalSeconds;
        if (elapsedseconds >= pingInterval)
        {
            waitingForPing = true;

            ping_m ping;
            ping.type = (ushort)message_t.ping;
            ping.random_number = (ushort)(UnityEngine.Random.value * 65536);

            pingTimer.Start();
            client.SendDataTcp(ping.bytes, ping_m.size);
        }
    }
    private void OnDestroy()
    {
        if (client != null)
            client.Disconnect();
        
    }

    public unsafe void LeaveLobby()
    {
        racing_lobby_action_m message;
        message.type = (ushort)message_t.racing_lobby_action;
        message.action = 3;
        client.SendDataTcp(message.bytes, racing_lobby_action_m.size);
        OnPlayerLeaveLobby.Invoke(player_username);
    }

    public unsafe void StartGame()
    {
        if (gameStarted) return;

        racing_lobby_action_m message;
        message.type = (ushort)message_t.racing_lobby_action;
        message.action = 5;
        client.SendDataTcp(message.bytes, racing_lobby_action_m.size);
        gameStarted = true;
    }

    public string GetPlayerName(int id)
    {
        return username_by_id.ContainsKey(id) ? username_by_id[id] : "404";
    }
}

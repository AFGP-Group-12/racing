using Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameplayClient : MonoBehaviour
{
    // Parameters
    public float playerUpdatesPerSecond = 10;
    public bool doPrediction = false;

    // Unity
    public static GameplayClient instance;
    public static GameObject player = null;
    public static GameObject mainCamera = null;
    public GameObject otherPlayerPrefab;
    private Dictionary<int, OtherPlayer> playerById;

    // Networking
    private Vector3 MaxWorldBounds = new Vector3(100, 100, 100);
    public BaseNetworkClient client = null;
    bool hasReceivedConnectionReply = false;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        Application.runInBackground = true;
    }

    public void Start()
    {
        playerById = new Dictionary<int, OtherPlayer>();
    }

    public void Setup()
    {
        client.ConnectToServer();
    }

    private void OnConnect()
    {
        Debug.Log("GameplayClient Connected!");
    }

    private void OnDestroy()
    {
        if (client != null)
            client.Disconnect();
    }

    private void Update()
    {
        if (client == null) { return; }
     
        generic_m message;
        while (client.TryDequeueMessage(out message))
        {
            switch (message.get_t())
            {
                case message_t.connection_reply:
                    hasReceivedConnectionReply = true;
                    break;
                case message_t.movement_reply:
                    HandleRecievedPlayerMovement(message.movement_reply);
                    break;
                case message_t.racing_game_start:
                    Debug.Log("Game Start!");
                    StartCoroutine(LoadGameScene());
                    break;
                default:
                    Debug.Log("Got Unexpected: " + message.get_t());
                    break;
            }
        }

        Physics.SyncTransforms();

        // update players positions
        foreach (OtherPlayer p in playerById.Values)
        {
            p.Update(doPrediction);
        }
    }


    IEnumerator LoadGameScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MovementScene");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log("Scene Loaded");

        yield return new WaitForSeconds(0.1f);

        player = GameObject.Find("Player");
        mainCamera = GameObject.Find("Main Camera");

        player.SetActive(false);

        yield return new WaitForSeconds(2);

        player.SetActive(true);

        StartCoroutine(SendPeriodicMessageCoroutine());
    }

private IEnumerator SendPeriodicMessageCoroutine()
    {
        while (!hasReceivedConnectionReply)
        {
            yield return new WaitForSeconds(0.05f);
        }

        client.BeginUdp(OnConnect);

        while (!client.IsConnectedUdp())
        {
            yield return new WaitForSeconds(0.05f);
        }


        while (client.IsConnectedUdp())
        {
            if (player != null)
            {
                SendMovementData(player.transform);
            }

            // Pause for the specified interval
            float messageInterval = 1 / playerUpdatesPerSecond;
            yield return new WaitForSeconds(messageInterval);
        }
        Debug.Log("client not connected");
    }

    private unsafe void SendMovementData(Transform transform)
    {
        movement_m m;
        m.type = (UInt16)message_t.movement;
        Helpers.fillPosition(transform.position, MaxWorldBounds, out m.position);
        Helpers.fillPosition(new Vector3(0,0,0), MaxWorldBounds, out m.velocity);
        if (mainCamera != null) Helpers.fillRotation(mainCamera.transform.rotation.eulerAngles.y, out m.rotation);
        client.SendDataUdp(m.bytes, movement_m.size);
    }
    private unsafe void HandleRecievedPlayerMovement(movement_m_reply message)
    {
        int id = message.from_id;
        if (!playerById.ContainsKey(id))
        {
            OnPlayerJoin(id);
            return;
        }

        Vector3 pos = Helpers.readPosition(message.position, MaxWorldBounds);
        Vector3 vel = Helpers.readPosition(message.velocity, MaxWorldBounds);
        double rotation = -1 * (double)(Helpers.readRotation(message.rotation) + 180) * Math.PI / 180.0;

        playerById[id].AddMovementReply(pos, vel, rotation);
    }

    private void OnPlayerJoin(int id)
    {
        if (playerById.ContainsKey(id))
        {
            playerById[id].Destroy();
            playerById.Remove(id);
        }
        playerById[id] = new OtherPlayer(otherPlayerPrefab, LobbyClient.instance.GetPlayerName(id), mainCamera.GetComponent<Camera>());
    }
    private void OnPlayerLeave(int id, string username)
    {
        if (playerById.ContainsKey(id))
        {
            playerById[id].Destroy();
            playerById.Remove(id);
        }

    }
}
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

    // Unity
    public static GameplayClient instance = null;
    public static GameObject player = null;
    public static GameObject mainCamera = null;
    public GameObject otherPlayerPrefab;
    public GameObject navMeshNode = null;
    public GameObject navMeshBounds = null;
    private Dictionary<int, OtherPlayer> playerById;

    private List<Vector3> navmeshnodes = new List<Vector3>();
    private List<int> navmeshtypes = new List<int>();
    private int totalNodes = 0;

    public MovementState CurrentState { get; set; }

    // Networking
    private Vector3 MaxWorldBounds = new Vector3(Int32.MaxValue / 100.0f, Int32.MaxValue / 100.0f, Int32.MaxValue / 100.0f);
    public BaseNetworkClient client = null;
    bool hasReceivedConnectionReply = false;
    bool isDead = false;

    // Abilities
    public Platform platformAbility;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        Application.runInBackground = true;
        
    }

    public void Start()
    {
        playerById = new Dictionary<int, OtherPlayer>();
        CurrentState = MovementState.idle;
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
                    StartCoroutine(SendPeriodicMessageCoroutine());
                    break;
                case message_t.movement_reply:
                    HandleRecievedPlayerMovement(message.movement_reply);
                    break;
                case message_t.racing_game_start:
                    Debug.Log("Game Start! Level: " + message.racing_game_start.level);
                    StartCoroutine(LoadGameScene(message.racing_game_start.level));
                    break;
                case message_t.racing_ability_action:
                    HandleRecievedAbilityAction(message.racing_ability_action);
                    break;
                case message_t.navmesh_data:
                    HandleNavmeshData(message);
                    break;
                default:
                    Debug.Log("Got Unexpected: " + message.get_t());
                    break;
            }
        }


        // update players positions
        foreach (OtherPlayer p in playerById.Values)
        {
            p.Update(false);
        }

        if (nodesPendingToSpawn()) spawnNodes();
        
        Physics.SyncTransforms();
    }

    private bool nodesPendingToSpawn()
    {
        return player != null && navmeshnodes.Count > 0;
    }

    private void spawnNodes()
    {
        for (int i = 0; i < navmeshnodes.Count; i++)
        {
            totalNodes++;
            if (navmeshtypes[i] == 0)
                Instantiate(navMeshNode, navmeshnodes[i], Quaternion.identity);
            else
                Instantiate(navMeshBounds, navmeshnodes[i], Quaternion.identity);
        }
        if (navmeshnodes.Count > 0)
        {
            Debug.Log("Total Navmesh Nodes Received: " + totalNodes);
        }
        navmeshnodes.Clear();
        navmeshtypes.Clear();
    }

    public void LoadSceneSinglePlayer(string scene)
    {
        StartCoroutine(LoadSceneSinglePlayerRoutine(scene));
    }

    private static IEnumerator LoadSceneSinglePlayerRoutine(string scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            Debug.Log("Loading Scene...");
            yield return null;
        }
        Debug.Log("Scene Loaded");

        yield return new WaitForSeconds(0.1f);

        player = GameObject.Find("Player");

        player.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        player.SetActive(true);
    }

    IEnumerator LoadGameScene(int level)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        string sceneName;
        switch (level)
        {
            case 0:
                sceneName = "Set1Level1";
                break;
            case 1:
                sceneName = "Set1Level2";
                break;
            case 2:
                sceneName = "Set1Level3";
                break;
            default:
                sceneName = "Set1Level1";
                break;
        }
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

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

        yield return new WaitForSeconds(0.1f);

        player.SetActive(true);

        //create a copy of the keys to avoid modification during iteration
        var playerIds = new List<int>(playerById.Keys);
        foreach (int id in playerIds) {
            OnPlayerLeave(id, "");
        }

        foreach (int id in playerIds) {
            OnPlayerJoin(id);
        }
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

    public unsafe void SendAbilityDataPlatform(Vector3 position)
    {
        if (client == null) return;
        racing_ability_action_m m;
        m.type = (UInt16)message_t.racing_ability_action;

        m.action = 1;
        Helpers.fillPosition(position, MaxWorldBounds, out m.position);

        client.SendDataTcp(m.bytes, racing_ability_action_m.size);
    }

    public unsafe void SendAbilityDataGrappleStationary(Vector3 position)
    { 
        if (client == null) return;
        racing_ability_action_m m;
        m.type = (UInt16)message_t.racing_ability_action;

        m.action = 2;
        Helpers.fillPosition(position, MaxWorldBounds, out m.position);

        client.SendDataTcp(m.bytes, racing_ability_action_m.size);
    }

    public unsafe void SendAbilityDataGrapplePlayer(int target_id)
    {
        if (client == null) return;
        racing_ability_action_m m;
        m.type = (UInt16)message_t.racing_ability_action;

        m.action = 3;
        m.target_player_id = (UInt16)target_id;

        client.SendDataTcp(m.bytes, racing_ability_action_m.size);
    }
    public unsafe void SendAbilityDataGrappleEnd()
    {
        if (client == null) return;
        racing_ability_action_m m;
        m.type = (UInt16)message_t.racing_ability_action;

        m.action = 4;
        
        client.SendDataTcp(m.bytes, racing_ability_action_m.size);
    }

    private void HandleRecievedAbilityAction(racing_ability_action_m message)
    {
        int id = message.from_id;
        if (!playerById.ContainsKey(id))
        {
            OnPlayerJoin(id);
            return;
        }

        switch(message.action)
        {
            case 0: // Null case
                Debug.Log("Got Unexpected null from received ability action");
                break;
            case 1: // Platform
                Vector3 pos = Helpers.readPosition(message.position, MaxWorldBounds);
                platformAbility.SpawnPlatform(pos);
                break;
            case 2: // Stationary Grapple
                pos = Helpers.readPosition(message.position, MaxWorldBounds);
                playerById[message.from_id].Grapple(pos);
                break;
            case 3: // Player Grapple
                int target = message.target_player_id;
                if (target == LobbyClient.instance.self_id)
                    playerById[message.from_id].Grapple(transform);
                else
                    playerById[message.from_id].Grapple(playerById[target]);
                break;
            case 4: // Any Grapple End
                playerById[message.from_id].EndGrapple();
                break;
            case 5: // Dead
                //killPlayer(message.target_player_id);
                break;
            default:
                Debug.Log("Got Unexpected ability action: " + message.action);
                break;
        }
    }
    private void killPlayer(int id)
    {
        if (id == LobbyClient.instance.self_id)
        {
            isDead = true;
            player.GetComponent<PlayerMovement>().isGhosted = true;
        } else if (playerById.ContainsKey(id))
        {
            playerById[id].turnGhost();
        } else
        {
            Debug.Log("Tried to kill non existent player with id: " + id);
        }
    }
    int prevFrame = 0;
    private unsafe void SendMovementData(Transform transform)
    {
        if (client == null) return;
        movement_m m;
        m.type = (UInt16)message_t.movement;
        Helpers.fillPosition(transform.position, MaxWorldBounds, out m.position);
        Helpers.fillPosition(new Vector3(0,0,0), MaxWorldBounds, out m.velocity);
        if (mainCamera != null) Helpers.fillRotation(mainCamera.transform.rotation.eulerAngles.y, out m.rotation);
        m.state = (ushort)CurrentState;
        client.SendDataUdp(m.bytes, movement_m.size);

        
        int currentFrame = Time.frameCount;
        Debug.Log("Movement sent frame delay: " + (currentFrame - prevFrame));
        prevFrame = currentFrame;
    }
    private void HandleRecievedPlayerMovement(movement_m_reply message)
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

        MovementState state = (MovementState)message.state;

        if (otherPlayerPrefab == null)
        {
            Debug.LogError("Other Player Prefab is not assigned in GameplayClient!");
            return;
        }
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not found in GameplayClient!");
            mainCamera = GameObject.Find("Main Camera");
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera is still not found in GameplayClient!");
                return;
            }
        }

        if (!playerById[id].AddMovementReply(pos, vel, rotation, state))
        {
            playerById[id].resetBot(otherPlayerPrefab, mainCamera.GetComponent<Camera>());
        }
    }

    private void OnPlayerJoin(int id)
    {
        if (playerById.ContainsKey(id))
        {
            if (playerById[id] != null) playerById[id].Destroy();
            playerById.Remove(id);
        }
        string othername = LobbyClient.instance.GetPlayerName(id);

        if (othername == "404")
        {
            othername = "bot_" + UnityEngine.Random.Range(1000, 9999);
        }
        if (otherPlayerPrefab == null)
        {
            Debug.LogError("Other Player Prefab is not assigned in GameplayClient!");
            return;
        }
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not found in GameplayClient!");
            return;
        }
        //if (othername != "404")
        playerById[id] = new OtherPlayer(otherPlayerPrefab, othername, mainCamera.GetComponent<Camera>(), id);
        Debug.Log("Player Joined: " + othername + " with id: " + id);
    }
    private void OnPlayerLeave(int id, string username)
    {
        if (playerById.ContainsKey(id))
        {
            if (playerById[id] != null) playerById[id].Destroy();
            playerById.Remove(id);
        }

    }

    private unsafe void HandleNavmeshData(generic_m message)
    {
        navmesh_data_m nav_mes = message.navmesh_data;
        const int positions_offset = 4;

        generic_m p = new generic_m();
        
        byte[] tmp = new byte[position_sm.size];
        for (int i = 0; i < nav_mes.n; i++)
        {
            for (int j = 0; j < position_sm.size; j++)
            {
                tmp[j] = message.bytes[positions_offset + i * position_sm.size + j];
            }
            p.from(tmp, position_sm.size);
            Vector3 pos = Helpers.readPosition(p.position, MaxWorldBounds);
            navmeshnodes.Add(pos);
            navmeshtypes.Add(nav_mes.node_types[i]);
        }
    }

    public int GetPlayerCount()
    {
        return playerById.Count;
    }
    public unsafe void PlayerReachedEndPoint()
    {
        Debug.Log("Notifying server of reaching endpoint");
        if (client == null) return;
        racing_game_reach_checkpoint_m m;
        m.type = (UInt16)message_t.racing_game_reach_checkpoint;

        client.SendDataTcp(m.bytes, racing_game_reach_checkpoint_m.size);
    }
}
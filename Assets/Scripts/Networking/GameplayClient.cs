using Messages;
using System;
using System.Collections.Generic;
using UnityEngine;


public class GameplayClient : MonoBehaviour
{
    public static GameplayClient instance;

    // Networking
    public BaseNetworkClient client = null;
    
    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void Setup()
    {
        client.ConnectToServer(OnConnect);
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

    private void FixedUpdate()
    {
        
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
                    client.BeginUdp();
                    break;
                default:
                    break;
            }
        }
    }
}

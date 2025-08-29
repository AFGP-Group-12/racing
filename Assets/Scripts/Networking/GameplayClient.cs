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
        client.Disconnect();
    }

    private void FixedUpdate()
    {
        
    }

    private void Update()
    {
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

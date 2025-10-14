using Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseNetworkClient
{
    // Server Info
    private string serverIp;
    private int serverPort;
    private int serverUdpPort;

    // Networking Objects
    private TcpClient tcpSocket = null;
    private UdpClient udpSocket = null;
    private NetworkStream tcpStream = null;
    
    private byte[] receiveBufferTcp;
    private int dataBufferSize;

    // Communication back to the main thread.
    private ConcurrentQueue<generic_m> incomming_message_queue;

    // Client State
    private bool connectedTcp = false;
    private bool connectedUdp = false;
    private bool tryingToConnectTcp = false;
    private bool tryingToConnectUdp = false;

    private UInt16 player_id;
    private string player_username;

    // Callbacks
    public delegate void ConnectionCallbacks();
    public delegate void DataReceivedCallback(generic_m m);
    
    private ConnectionCallbacks onConnectTcpUserCallback;
    private ConnectionCallbacks onConnectUdpUserCallback;

    /*
     * Class Constructor - Initializes server information and some state. 
     */
    public BaseNetworkClient(string ip, int port, int bufferSize, string username, UInt16 p_id = 0)
    {
        serverIp = ip;
        serverPort = port;

        dataBufferSize = bufferSize;

        tcpSocket = new TcpClient
        {
            ReceiveBufferSize = dataBufferSize,
            SendBufferSize = dataBufferSize
        };
        receiveBufferTcp = new byte[dataBufferSize];

        incomming_message_queue = new ConcurrentQueue<generic_m>();

        player_username = username;
        player_id = p_id;
    }

    // Initiates a tcp connection to the server.
    public void ConnectToServer(ConnectionCallbacks onConnectTcpUserCallback = null)
    {
        if (connectedTcp || tryingToConnectTcp) { return; }

        if (onConnectTcpUserCallback != null) this.onConnectTcpUserCallback += onConnectTcpUserCallback;
        tryingToConnectTcp = true;
        
        tcpSocket.BeginConnect(serverIp, serverPort, TcpConnectCallback, tcpSocket);
    }

    // Requests a udp connection from the server via the tcp connection. Requires an already existing tcp connection to the server.
    public unsafe void BeginUdp(ConnectionCallbacks onConnectUdpUserCallback = null)
    {
        if (!connectedTcp || connectedUdp || tryingToConnectUdp) { return; }

        udpSocket = new UdpClient(0, AddressFamily.InterNetwork);
        if (onConnectUdpUserCallback != null) this.onConnectUdpUserCallback += onConnectUdpUserCallback;

        initiate_udp_m m;
        m.type = (UInt16)message_t.initiate_udp;
        try
        {
            // Port is opened when the udp socket is created.
            m.port = (UInt16)((IPEndPoint)udpSocket.Client.LocalEndPoint).Port;
        }
        catch (Exception e) { Debug.LogError(e.Message); }

        SendDataTcp(m.bytes, initiate_udp_m.size);
        
        tryingToConnectUdp = true;
    }

    public unsafe void SendDataTcp(byte* data, int len)
    {
        if (!connectedTcp) return;

        byte[] dataArr = new byte[len];
        for (int i = 0; i < len; i++)
        {
            dataArr[i] = data[i];
        }

        tcpStream.BeginWrite(dataArr, 0, dataArr.Length, null, null);
    }

    public unsafe void RegisterUdp()
    {
        racing_ability_action_m m;
        m.type = (UInt16)message_t.racing_ability_action;

        m.from_id = player_id;

        byte[] dataArr = new byte[racing_ability_action_m.size];
        for (int i = 0; i < racing_ability_action_m.size; i++)
        {
            dataArr[i] = m.bytes[i];
        }


        udpSocket.Send(dataArr, dataArr.Length, serverIp, 8081);//, target);
    }

    public unsafe void SendDataUdp(byte* data, int len)
    {
        //IPEndPoint target = new IPEndPoint(IPAddress.Parse(serverIp), 8081);
        //IPEndPoint target = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8085);

        if (!connectedUdp) return;

        byte[] dataArr = new byte[len];
        for (int i = 0; i < len; i++)
        {
            dataArr[i] = data[i];
        }

        try
        {
            udpSocket.Send(dataArr, dataArr.Length, serverIp, serverUdpPort);//, target);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public bool TryDequeueMessage(out generic_m message)
    {
        return incomming_message_queue.TryDequeue(out message);
    }

    public void Disconnect()
    {
        if (connectedTcp)
        {
            tcpSocket.Close();
            connectedTcp = false;
        }
        if (connectedUdp)
        {
            udpSocket.Close();
            connectedUdp = false;
        }
    }

    public bool IsConnectedTcp()
    {
        return connectedTcp;
    }

    public bool IsConnectedUdp()
    {
        return connectedUdp;
    }

    public bool IsTryingToConnectTcp()
    {
        return tryingToConnectTcp;
    }

    public bool IsTryingToConnectUdp()
    {
        return tryingToConnectUdp;
    }

    private unsafe void TcpConnectCallback(IAsyncResult result)
    {
        try
        {
            tcpSocket.EndConnect(result);

            if (!tcpSocket.Connected)
            {
                connectedTcp = false;
                tryingToConnectTcp = false;
                Debug.Log("TcpConnectCallback wasn't able to connect.");
                return;
            }

            tcpStream = tcpSocket.GetStream();

            connectedTcp = true;
            tryingToConnectTcp = false;

            connection_m m;
            m.type = (UInt16)message_t.connection;
            m.key[0] = (byte)'a';
            m.key[1] = (byte)'e';
            m.key[2] = (byte)'7';
            m.key[3] = (byte)'0';
            m.key[4] = (byte)'m';

            Messages.Helpers.fillUsername(player_username, out m.username);
            m.id = player_id;

            SendDataTcp(m.bytes, connection_m.size);

            if (onConnectTcpUserCallback != null) onConnectTcpUserCallback();
            readAgainTcp();
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}{e.StackTrace}");
        }
    }

    private void UdpConnectCallback()
    {
        try
        {
            connectedUdp = true;
            tryingToConnectUdp = false;
            if (onConnectUdpUserCallback != null) onConnectUdpUserCallback();
            readAgainUdp();
        } catch (Exception e)
        {
            Debug.LogError($"{e.Message}{e.StackTrace}");
        }
    }

    private unsafe void ReceiveCallbackTcp(IAsyncResult result)
    {
        try
        {
            if (!connectedTcp) { return; }

            // We will assume that we read in whole messages for now...
            int n = tcpStream.EndRead(result);
            int read_offset = 0;

            while (n > 0)
            {
                message_t m_type = Messages.Helpers.GetMessageType(receiveBufferTcp, read_offset);
                int message_len = Messages.Helpers.getMessageSize(m_type);

                // Handle error  cases.
                if (message_len == -1)
                {
                    Debug.Log("Type not registered: " + m_type);
                    readAgainTcp();
                    return;
                }

                if (n < message_len)
                {
                    Debug.Log("Got incomplete message");
                    readAgainTcp();
                    return;
                }

                // Generate what the user will read.
                generic_m m = new generic_m();
                m.from(receiveBufferTcp, message_len, read_offset);

                read_offset += message_len;
                n -= message_len;

                bool skipUser = false; // Some messages will not be shown to the user.

                if (m_type == message_t.connection_reply)
                {
                    player_id = m.connection_reply.id;
                }

                // Server telling client to connect to a different server. In this case, the gameplay server.
                if (m_type == message_t.server_info)
                {
                    skipUser = true; // Not handled by user. 
                    string server_addr = System.Text.Encoding.UTF8.GetString(m.server_info.address, 32);
                    int port = m.server_info.port;

                    // Debug.Log($"Got some server info. Addr: {server_addr}, Port: {port}, Type: {(server_t)m.server_info.server_name}");

                    if ((server_t)m.server_info.name == server_t.gameplay_server && GameplayClient.instance.client == null)
                    {
                        if (server_addr.Trim().Contains("127.0.0.1")) { server_addr = serverIp; }

                        GameplayClient.instance.client = new BaseNetworkClient(server_addr, port, dataBufferSize, player_username, player_id);
                        GameplayClient.instance.Setup();
                    }
                }

                // Server has created a udp socket and informing the client its ready to connect.
                if (m_type == message_t.initiate_udp_reply)
                {
                    skipUser = true; // Not handled by user. 
                    if (tryingToConnectUdp)
                    {
                        if (m.initiate_udp_reply.granted != 0)
                        {
                            // Recieve the message
                            serverUdpPort = m.initiate_udp_reply.port;
                            UdpConnectCallback();
                        }
                        else
                        {
                            Debug.Log("Udp connection request was rejected");
                        }
                    }
                    else
                    {
                        Debug.LogError("Received initiate_udp_reply while not trying to connect udp");
                    }
                }

                if (!skipUser)
                {
                    incomming_message_queue.Enqueue(m);
                }
            }

            readAgainTcp();
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}{e.StackTrace}");
        }
    }

    private unsafe void ReceiveCallbackUdp(IAsyncResult result)
    {
        try
        {
            if (!connectedTcp || !connectedUdp) { return; }

            System.Net.IPEndPoint endpoint = null;
            byte[] data = udpSocket.EndReceive(result, ref endpoint);

            if (data.Length <= 0)
            {
                Debug.Log("Received " + data.Length + " size for tcpStream.EndRead()");
                readAgainUdp();
                return;
            }

            // Udp Packets are discrete.
            generic_m m = new generic_m();
            m.from(data, data.Length);

            message_t m_type = m.get_t();

            incomming_message_queue.Enqueue(m);

            readAgainUdp();

        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}{e.StackTrace}");
        }
    }
    public void changeUsername(string user)
    {
        player_username = user;
    }

    private void readAgainTcp()
    {
        tcpStream.BeginRead(receiveBufferTcp, 0, dataBufferSize, ReceiveCallbackTcp, null);
    }

    private void readAgainUdp()
    {
        udpSocket.BeginReceive(ReceiveCallbackUdp, udpSocket);
    }
}

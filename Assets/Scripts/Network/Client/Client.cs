using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.Events;
using System;

[Serializable]
public class ClientEvent : UnityEvent { };
[Serializable]
public class ClientInitEvent : UnityEvent<InitMessage> { };
[Serializable]
public class ClientPlayerAddEvent : UnityEvent<PlayerAddMessage> { };
[Serializable]
public class ClientPlayerRemoveEvent : UnityEvent<PlayerRemoveMessage> { };
[Serializable]
public class ClientEntityAddEvent : UnityEvent<EntityAddMessage> { };
[Serializable]
public class ClientEntityRemoveEvent : UnityEvent<EntityRemoveMessage> { };
[Serializable]
public class ClientStateEvent : UnityEvent<StateMessage> { };
[Serializable]
public class ClientLANDiscoveredEvent : UnityEvent<string, IPEndPoint> { };

public class Client : MonoBehaviour, INetEventListener
{
    [Header("Network Settings")]
    public string serverIP="192.168.0.x";
    public int serverPort=5000;
    public string token = "appsecret";
    public bool autoConnect;
    [Header("Monitoring")]
    [ReadOnly]
    public bool connected;
    [ReadOnly]
    public int lastSentPacketSize;
    [Header("Client Events")]
    public ClientLANDiscoveredEvent onLANDiscovered = new ClientLANDiscoveredEvent();
    public ClientEvent onConnected = new ClientEvent();
    public ClientEvent onDisconnected = new ClientEvent();
    public ClientInitEvent onInit = new ClientInitEvent();
    public ClientPlayerAddEvent onPlayerAdd = new ClientPlayerAddEvent();
    public ClientPlayerRemoveEvent onPlayerRemove = new ClientPlayerRemoveEvent();
    public ClientEntityAddEvent onEntityAdd = new ClientEntityAddEvent();
    public ClientEntityRemoveEvent onEntityRemove = new ClientEntityRemoveEvent();
    public ClientStateEvent onState = new ClientStateEvent();
    private NetManager _netClient;
    private NetPeer server;
    private NetDataWriter _dataWriter;
    private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();
    private bool _discovering = false;
    private float _discoveringInterval = 0.5f;
    private float _discoveringTimer = 0;

    void Start()
    {
        _dataWriter = new NetDataWriter();
        _netClient = new NetManager(this);
        _netClient.UnconnectedMessagesEnabled = true;
        _netClient.UpdateTime = 15;
        _netClient.Start();
        _netPacketProcessor.RegisterNestedType(Vector3Utils.Serialize, Vector3Utils.Deserialize);
        _netPacketProcessor.RegisterNestedType(QuatUtils.Serialize, QuatUtils.Deserialize);
        _netPacketProcessor.RegisterNestedType(() => new PlayerInput());
        _netPacketProcessor.RegisterNestedType(() => new PlayerState());
        _netPacketProcessor.RegisterNestedType(() => new EntityState());
        _netPacketProcessor.SubscribeReusable((InitMessage im, NetPeer np) => onInit.Invoke(im));
        _netPacketProcessor.SubscribeReusable((PlayerAddMessage pam, NetPeer np) => onPlayerAdd.Invoke(pam));
        _netPacketProcessor.SubscribeReusable((PlayerRemoveMessage prm, NetPeer np) => onPlayerRemove.Invoke(prm));
        _netPacketProcessor.SubscribeReusable((EntityAddMessage eam, NetPeer np) => onEntityAdd.Invoke(eam));
        _netPacketProcessor.SubscribeReusable((EntityRemoveMessage erm, NetPeer np) => onEntityRemove.Invoke(erm));
        _netPacketProcessor.SubscribeReusable((StateMessage sm, NetPeer np) => onState.Invoke(sm));
        if (autoConnect)
        {
            Connect(new IPEndPoint(IPAddress.Parse(serverIP), serverPort));
        }
    }

    void Update()
    {
        _netClient.PollEvents();
        NetPeer peer = _netClient.FirstPeer;
        if (peer != null)
        {
            _discovering = false;
        }
        if (_discovering)
        {
            _discoveringTimer += Time.deltaTime;
            if (_discoveringTimer >= _discoveringInterval)
            {
                //Debug.Log("SENDING DISCOVERY PACKET");
                _netClient.SendBroadcast(new byte[] { 1 }, 5000);
                _discoveringTimer = 0;
            }
        }
    }

    void OnDestroy()
    {
        if (_netClient != null)
            _netClient.Stop();
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("Connected to " + peer.EndPoint + " with id: " + peer.Id);
        server = peer;
        connected = true;
        onConnected.Invoke();
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log("Network error " + socketErrorCode);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        _netPacketProcessor.ReadAllPackets(reader, peer);
    }

    //private void OnInitReceived(InitMessage im, NetPeer peer)
    //{
    //    onInit.Invoke(im);
    //}

    //private void OnStateReceived(StateMessage sm, NetPeer peer)
    //{
    //    onState.Invoke(sm);
    //}

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.BasicMessage && _netClient.ConnectedPeersCount == 0)
        {
            onLANDiscovered.Invoke(reader.GetString(), remoteEndPoint);
        }
    }

    public void Connect(IPEndPoint endPoint)
    {
        _netClient.Connect(endPoint, token);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    public void OnConnectionRequest(ConnectionRequest request)
    {

    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
        connected = false;
        server = null;
        onDisconnected.Invoke();
    }

    public void StartLANDiscovery()
    {
        _discovering = true;
    }
    public void StopLANDiscovery()
    {
        _discovering = false;
    }
    public void SendImportantMessage<T>(T data) where T : class, new()
    {
        _netPacketProcessor.Send(server, data, DeliveryMethod.ReliableOrdered);
    }
    public void SendFastMessage<T>(T data) where T : class, new()
    {
        byte[] ba = _netPacketProcessor.Write(data);
        lastSentPacketSize = ba.Length;
        _netPacketProcessor.Send(_netClient, data, DeliveryMethod.Unreliable);
    }
}
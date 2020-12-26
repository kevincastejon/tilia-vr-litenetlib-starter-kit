using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.Events;
using System;

[Serializable]
public class ServerPlayerEvent : UnityEvent<int> { };
[Serializable]
public class ServerPlayerInputEvent : UnityEvent<int, PlayerInput> { };

public class Server : MonoBehaviour, INetEventListener, INetLogger
{
    [Header("Server Events")]
    public ServerPlayerEvent onPlayerConnected = new ServerPlayerEvent();
    public ServerPlayerEvent onPlayerDisconnected = new ServerPlayerEvent();
    public ServerPlayerInputEvent onPlayerInput = new ServerPlayerInputEvent();
    [Header("Network Settings")]
    public string token = "appsecret";
    [Header("Monitoring")]
    [ReadOnly]
    public int lastSentPacketSize;
    private NetManager _netServer;
    private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();

    void Start()
    {
        NetDebug.Logger = this;
        _netPacketProcessor.RegisterNestedType(Vector3Utils.Serialize, Vector3Utils.Deserialize);
        _netPacketProcessor.RegisterNestedType(QuatUtils.Serialize, QuatUtils.Deserialize);
        _netPacketProcessor.RegisterNestedType(() => new PlayerState());
        _netPacketProcessor.RegisterNestedType(() => new EntityState());
        _netPacketProcessor.SubscribeReusable<PlayerInput, NetPeer>(OnPlayerInput);
    }

    void Update()
    {
        _netServer.PollEvents();
    }

    void OnDestroy()
    {
        NetDebug.Logger = null;
        if (_netServer != null)
            _netServer.Stop();
    }

    public void Listen(int port)
    {
        _netServer = new NetManager(this);
        _netServer.Start(port);
        _netServer.BroadcastReceiveEnabled = true;
        _netServer.UpdateTime = 15;
    }

    public NetPeer GetPeerById(int peerId)
    {
        return(_netServer.GetPeerById(peerId));
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[SERVER] We have new peer " + peer.EndPoint + " with id : "+peer.Id);
        onPlayerConnected.Invoke(peer.Id);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log("[SERVER] error " + socketErrorCode);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.Broadcast)
        {
            Debug.Log("[SERVER] Received discovery request. Send discovery response");
            NetDataWriter resp = new NetDataWriter();
            resp.Put("Partie de test");
            _netServer.SendUnconnectedMessage(resp, remoteEndPoint);
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        //request.Accept();
        request.AcceptIfKey(token);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
        onPlayerDisconnected.Invoke(peer.Id);
    }

    private void OnPlayerInput(PlayerInput pi, NetPeer peer)
    {
        //Debug.Log("received player state");
        onPlayerInput.Invoke(peer.Id, pi);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        _netPacketProcessor.ReadAllPackets(reader, peer);
    }

    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        Debug.LogFormat(str, args);
    }
    public void SendImportantMessage<T>(T data) where T : class, new()
    {
        SendImportantMessage(data, -1, false);
    }
    public void SendImportantMessage<T>(T data, int peerId) where T : class, new()
    {
        SendImportantMessage(data, peerId, false);
    }
    public void SendImportantMessage<T>(T data, int peerId, bool exclusion) where T : class, new()
    {
        byte[] ba = _netPacketProcessor.Write(data);
        if (peerId != -1)
        {
            if (exclusion)
            {
                _netServer.SendToAll(ba, DeliveryMethod.ReliableOrdered, GetPeerById(peerId));
            }
            else
            {
                _netPacketProcessor.Send(GetPeerById(peerId), data, DeliveryMethod.ReliableOrdered);
            }
        }
        else
        {
            _netPacketProcessor.Send(_netServer, data, DeliveryMethod.ReliableOrdered);
        }
    }
    public void SendFastMessage<T>(T data) where T : class, new()
    {
        SendFastMessage(data, -1, false);
    }
    public void SendFastMessage<T>(T data, int peerId) where T : class, new()
    {
        SendFastMessage(data, peerId, false);
    }
    public void SendFastMessage<T>(T data, int peerId, bool exclusion) where T : class, new()
    {
        byte[] ba = _netPacketProcessor.Write(data);
        lastSentPacketSize=ba.Length;
        if (peerId != -1)
        {
            if (exclusion)
            {
                _netServer.SendToAll(ba, DeliveryMethod.Unreliable, GetPeerById(peerId));
            }
            else
            {
                _netPacketProcessor.Send(GetPeerById(peerId), data, DeliveryMethod.Unreliable);
            }
        }
        else
        {
            _netPacketProcessor.Send(_netServer, data, DeliveryMethod.Unreliable);
        }
    }
}
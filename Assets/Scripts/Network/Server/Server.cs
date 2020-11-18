using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.Events;

public class BaseServerPlayerEvent : UnityEvent<NetPeer> { };
public class BaseServerPlayerStateEvent : UnityEvent<NetPeer, PlayerState> { };

public class Server : MonoBehaviour, INetEventListener, INetLogger
{
    public BaseServerPlayerEvent onPlayerConnected = new BaseServerPlayerEvent();
    public BaseServerPlayerEvent onPlayerDisconnected = new BaseServerPlayerEvent();
    public BaseServerPlayerStateEvent onPlayerState = new BaseServerPlayerStateEvent();
    public string token = "appsecret";
    private NetManager _netServer;
    private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();

    void Start()
    {
        NetDebug.Logger = this;
        _netPacketProcessor.RegisterNestedType(Vector3Utils.Serialize, Vector3Utils.Deserialize);
        _netPacketProcessor.RegisterNestedType(QuatUtils.Serialize, QuatUtils.Deserialize);
        _netPacketProcessor.RegisterNestedType(() => new PlayerState());
        _netPacketProcessor.RegisterNestedType(() => new EntityState());
        _netPacketProcessor.SubscribeReusable<PlayerState, NetPeer>(OnPlayerState);
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
        onPlayerConnected.Invoke(peer);
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
        onPlayerDisconnected.Invoke(peer);
    }

    private void OnPlayerState(PlayerState ps, NetPeer peer)
    {
        Debug.Log("received player state");
        onPlayerState.Invoke(peer, ps);
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
        SendImportantMessage(data, null, false);
    }
    public void SendImportantMessage<T>(T data, NetPeer peer) where T : class, new()
    {
        SendImportantMessage(data, peer, false);
    }
    public void SendImportantMessage<T>(T data, NetPeer peer, bool exclusion) where T : class, new()
    {
        byte[] ba = _netPacketProcessor.Write(data);
        Debug.Log(ba.Length + "b");
        if (peer != null)
        {
            if (exclusion)
            {
                _netServer.SendToAll(ba, DeliveryMethod.ReliableOrdered, peer);
            }
            else
            {
                _netPacketProcessor.Send(peer, data, DeliveryMethod.ReliableOrdered);
            }
        }
        else
        {
            _netPacketProcessor.Send(_netServer, data, DeliveryMethod.ReliableOrdered);
        }
    }
    public void SendFastMessage<T>(T data) where T : class, new()
    {
        SendFastMessage(data, null, false);
    }
    public void SendFastMessage<T>(T data, NetPeer peer) where T : class, new()
    {
        SendFastMessage(data, peer, false);
    }
    public void SendFastMessage<T>(T data, NetPeer peer, bool exclusion) where T : class, new()
    {
        byte[] ba = _netPacketProcessor.Write(data);
        //Debug.Log(ba.Length + "b");
        if (peer != null)
        {
            if (exclusion)
            {
                _netServer.SendToAll(ba, DeliveryMethod.Sequenced, peer);
            }
            else
            {
                _netPacketProcessor.Send(peer, data, DeliveryMethod.Sequenced);
            }
        }
        else
        {
            _netPacketProcessor.Send(_netServer, data, DeliveryMethod.Sequenced);
        }
    }
}
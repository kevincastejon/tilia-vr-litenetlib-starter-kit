using LiteNetLib;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerEvent : UnityEvent<int> { };
[Serializable]
public class PlayerStateEvent : UnityEvent<int, PlayerState> { };

public class GameServer : MonoBehaviour
{
    public int port = 5000;
    public bool autoStart = true;
    public PlayerEvent onPlayerConnected = new PlayerEvent();
    public PlayerEvent onPlayerDisconnected = new PlayerEvent();
    public PlayerStateEvent onPlayerState = new PlayerStateEvent();
    private Server server;
    // Start is called before the first frame update
    void Start()
    {
        server = GetComponentInChildren<Server>();
        server.onPlayerConnected.AddListener((NetPeer peer) => OnPeerConnected(peer));
        server.onPlayerDisconnected.AddListener((NetPeer peer) => OnPeerDisconnected(peer));
        server.onPlayerState.AddListener((NetPeer peer, PlayerState ps) => OnPeerState(peer, ps));
        if (autoStart)
        {
            Listen();
            Debug.Log("Server listening on port " + port);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
    private void OnDestroy()
    {
        server.onPlayerConnected.RemoveAllListeners();
        server.onPlayerConnected.RemoveAllListeners();
        server.onPlayerState.RemoveAllListeners();
    }
    private void Listen()
    {
        server.Listen(port);
    }
    private void OnPeerConnected(NetPeer peer)
    {
        onPlayerConnected.Invoke(peer.Id);
    }
    private void OnPeerDisconnected(NetPeer peer)
    {
        onPlayerDisconnected.Invoke(peer.Id);
    }
    private void OnPeerState(NetPeer peer, PlayerState ps)
    {
        onPlayerState.Invoke(peer.Id, ps);
    }
    public void SendInitMessage(InitMessage im, int peerId)
    {
        Debug.Log("SENT INIT MESSAGE");
        server.SendImportantMessage(im, server.GetPeerById(peerId));
    }
    public void SendWorldState(StateMessage sm)
    {
        server.SendFastMessage(sm);
    }
}

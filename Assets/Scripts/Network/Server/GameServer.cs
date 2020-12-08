using LiteNetLib;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerEvent : UnityEvent<int> { };
[Serializable]
public class PlayerInputEvent : UnityEvent<int, PlayerInput> { };

public class GameServer : MonoBehaviour
{
    public int port = 5000;
    public bool autoStart = true;
    public PlayerEvent onPlayerConnected = new PlayerEvent();
    public PlayerEvent onPlayerDisconnected = new PlayerEvent();
    public PlayerInputEvent onPlayerInput = new PlayerInputEvent();
    //private Server server;
    // Start is called before the first frame update
    void Awake()
    {
        //server = GetComponentInChildren<Server>();
        //server.onPlayerConnected.AddListener((NetPeer peer) => OnPeerConnected(peer));
        //server.onPlayerDisconnected.AddListener((NetPeer peer) => OnPeerDisconnected(peer));
        //server.onPlayerInput.AddListener((NetPeer peer, PlayerInput pi) => OnPeerInput(peer, pi));
        //if (autoStart)
        //{
        //    Listen();
        //    Debug.Log("Server listening on port " + port);
        //}
    }
    // Update is called once per frame
    void Update()
    {

    }
    private void OnDestroy()
    {
        //server.onPlayerConnected.RemoveAllListeners();
        //server.onPlayerConnected.RemoveAllListeners();
        //server.onPlayerInput.RemoveAllListeners();
    }
    private void Listen()
    {
        //server.Listen(port);
    }
    private void OnPeerConnected(NetPeer peer)
    {
        onPlayerConnected.Invoke(peer.Id);
    }
    private void OnPeerDisconnected(NetPeer peer)
    {
        onPlayerDisconnected.Invoke(peer.Id);
    }
    private void OnPeerInput(NetPeer peer, PlayerInput pi)
    {
        onPlayerInput.Invoke(peer.Id, pi);
    }
    public void SendInitMessage(InitMessage im, int peerId)
    {
        //Debug.Log("SENT INIT MESSAGE");
        //server.SendImportantMessage(im, server.GetPeerById(peerId));
    }
    public void SendWorldState(StateMessage sm, int peerId)
    {
        //server.SendFastMessage(sm, peerId);
    }
}

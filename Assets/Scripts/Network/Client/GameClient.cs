using LiteNetLib;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
[Serializable]
public class ClientEvent : UnityEvent { };
[Serializable]
public class ClientInitEvent : UnityEvent<InitMessage> { };
[Serializable]
public class ClientStateEvent : UnityEvent<StateMessage> { };
[Serializable]
public class ClientLANDiscoveredEvent : UnityEvent<string, IPEndPoint> { };

public class GameClient : MonoBehaviour
{
    public string serverIP;
    public int serverPort;
    public bool autoConnect;
    public ClientLANDiscoveredEvent onLANDiscovered = new ClientLANDiscoveredEvent();
    public ClientEvent onConnected = new ClientEvent();
    public ClientEvent onDisconnected = new ClientEvent();
    public ClientInitEvent onInit = new ClientInitEvent();
    public ClientStateEvent onState = new ClientStateEvent();
    private Client client;

    public bool Connected => (client.Connected);

    // Start is called before the first frame update
    void Start()
    {
        client = GetComponentInChildren<Client>();
        client.onLANDiscovered.AddListener((string name, IPEndPoint ip) => OnLANDiscovered(name, ip));
        client.onConnected.AddListener(() => OnConnected());
        client.onDisconnected.AddListener(() => OnDisconnected());
        client.onInit.AddListener((InitMessage im) => OnInit(im));
        client.onState.AddListener((StateMessage sm) => OnState(sm));
        if (autoConnect)
        {
            Connect(new IPEndPoint(IPAddress.Parse(serverIP), serverPort));
        }
    }
    /// <summary>
    /// Start sending regular discovery broadcast message
    /// </summary>
    public void StartLANDiscovery()
    {
        client.StartDiscovery();
    }
    /// <summary>
    /// Stop sending regular discovery broadcast message
    /// </summary>
    public void StopLANDiscovery()
    {
        client.StopDiscovery();
    }
    /// <summary>
    /// Tries to connect to a server with the specified endPoint and token
    /// </summary>
    public void Connect(IPEndPoint endPoint)
    {
        client.Connect(endPoint);
    }

    /// <summary>
    /// Sends the local player's state
    /// </summary>
    public void SendAvatarState(AvatarState avatarState)
    {
        client.SendFastMessage(avatarState);
    }
    private void OnLANDiscovered(string name, IPEndPoint ip)
    {
        onLANDiscovered.Invoke(name, ip);
    }

    private void OnConnected()
    {
        onConnected.Invoke();
    }

    private void OnDisconnected()
    {
        onDisconnected.Invoke();
    }

    private void OnInit(InitMessage im)
    {
        Debug.Log("RECEIVED INIT MESSAGE");
        onInit.Invoke(im);
    }

    private void OnState(StateMessage sm)
    {
        onState.Invoke(sm);
    }
}

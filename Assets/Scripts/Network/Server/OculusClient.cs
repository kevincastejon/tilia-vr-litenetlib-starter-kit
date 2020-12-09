using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

public class OculusClient : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Core.Initialize();
        Entitlements.IsUserEntitledToApplication().OnComplete(UserEntitled);
        Matchmaking.SetMatchFoundNotificationCallback(RoomFound);
        //Rooms.SetUpdateNotificationCallback(RoomUpdated);
        Net.SetPeerConnectRequestCallback(OnServerInvite);
        Net.SetConnectionStateChangedCallback(OnConnectedToServer);
    }

    private void OnConnectedToServer(Message<NetworkingPeer> msg)
    {
        if (msg.Data.State == PeerConnectionState.Connected)
        {
            Debug.Log("CONNECTED TO SERVER WITH ID " + msg.Data.ID);
        }
        else if (msg.Data.State == PeerConnectionState.Closed || msg.Data.State == PeerConnectionState.Timeout)
        {
            Debug.Log("DISCONNECTED FROM SERVER WITH ID " + msg.Data.ID);
        }
    }

    private void OnServerInvite(Message<NetworkingPeer> msg)
    {
        Net.Accept(msg.Data.ID);
    }

    private void UserEntitled(Message msg)
    {
        if (!msg.IsError)
        {
            Debug.Log("You are entitled to use this app");
            Users.GetLoggedInUser().OnComplete(OnUserInfo);
        }
        else
        {
            Debug.Log("error: You are not entitled to use this app");
        }
    }

    private void OnUserInfo(Message<User> msg)
    {
        Debug.Log("LOGGED IN AS " + msg.Data.ID + " " + msg.Data.OculusID);
        Matchmaking.Enqueue2("piootabouret");
    }

    private void RoomFound(Message<Room> msg)
    {
        RoomOptions opt = new RoomOptions();
        Rooms.Join2(msg.Data.ID, opt).OnComplete(RoomJoined);
    }

    private void RoomJoined(Message<Room> msg)
    {
        Debug.Log("ROOM "+msg.Data.ID+" JOINED ");
    }

    private void RoomUpdated(Message<Room> msg)
    {
        Debug.Log("ROOM UPDATED");
        for (int i = 0; i < msg.Data.UsersOptional.Count; i++)
        {
            User user = msg.Data.UsersOptional[i];
            Debug.Log("- "+user.ID+" "+user.OculusID);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

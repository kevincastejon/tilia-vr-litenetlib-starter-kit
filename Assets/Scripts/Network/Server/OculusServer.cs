using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

public class OculusServer : MonoBehaviour
{
    [ReadOnly]
    public ulong ID;
    [ReadOnly]
    public string oculusID;
    [ReadOnly]
    public List<User> roomClients = new List<User>();
    // Start is called before the first frame update
    void Start()
    {
        Core.Initialize();
        Entitlements.IsUserEntitledToApplication().OnComplete(UserEntitled);
        Rooms.SetUpdateNotificationCallback(RoomUpdated);
        Net.SetConnectionStateChangedCallback(OnClientConnected);
    }

    private void OnClientConnected(Message<NetworkingPeer> msg)
    {
        if (msg.Data.State == PeerConnectionState.Connected)
        {
            Debug.Log("USER CONNECTED WITH ID "+msg.Data.ID);
        }
        else if(msg.Data.State == PeerConnectionState.Closed || msg.Data.State == PeerConnectionState.Timeout)
        {
            Debug.Log("USER DISCONNECTED WITH ID " + msg.Data.ID);
        }
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
        ID = msg.Data.ID;
        oculusID = msg.Data.OculusID;
        Matchmaking.CreateAndEnqueueRoom2("piootabouret").OnComplete(RoomCreated);
    }

    private void RoomCreated(Message<MatchmakingEnqueueResultAndRoom> msg)
    {
        if (!msg.IsError)
        {
            Debug.Log("ROOM CREATED WITH ID " + msg.Data.Room.ID);
        }
        else
        {
            Debug.Log("error: Failed to create and enqueue room - " + msg.GetError().Message);
        }
    }

    private void RoomUpdated(Message<Room> msg)
    {
        Debug.Log("ROOM UPDATED");
        for (int i = 0; i < msg.Data.UsersOptional.Count; i++)
        {
            User user = msg.Data.UsersOptional[i];
            Debug.Log("- " + user.ID + " " + user.OculusID);
            if (user.ID == ID)
            {
                continue;
            }
            User existingUser = roomClients.Find(u => u.ID == user.ID);
            if (existingUser == null)
            {
                OnClientJoined(user);
            }
        }
        for (int i = 0; i < roomClients.Count; i++)
        {
            User user = roomClients[i];
            bool stillConnected = false;
            for (int j = 0; j < msg.Data.UsersOptional.Count; j++)
            {
                User u = msg.Data.UsersOptional[j];
                if (u.ID == ID)
                {
                    continue;
                }
                if (u.ID == user.ID)
                {
                    stillConnected = true;
                    break;
                }
            }
            if (!stillConnected)
            {
                OnClientLeaved(user);
            }
        }
    }

    private void OnClientJoined(User user)
    {
        Debug.Log("USER JOINED ROOM WITH ID " + user.ID);
        roomClients.Add(user);
        Net.Connect(user.ID);
    }

    private void OnClientLeaved(User user)
    {
        Debug.Log("USER LEAVED ROOM WITH ID " + user.ID);
        roomClients.Remove(user);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

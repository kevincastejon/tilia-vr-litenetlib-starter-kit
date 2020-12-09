using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

public class OculusServer : MonoBehaviour
{
    [ReadOnly]
    public List<User> clients = new List<User>();
    // Start is called before the first frame update
    void Start()
    {
        Core.Initialize();
        Entitlements.IsUserEntitledToApplication().OnComplete(UserEntitled);
        Rooms.SetUpdateNotificationCallback(RoomUpdated);
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
            User existingUser = clients.Find(u => u.ID == user.ID);
            if (existingUser == null)
            {
                OnClientJoined(user);
            }
        }
        for (int i = 0; i < clients.Count; i++)
        {
            User user = clients[i];
            bool alreadyConnected = false;
            for (int j = 0; j < msg.Data.UsersOptional.Count; j++)
            {
                User u = msg.Data.UsersOptional[j];
                if (u.ID == user.ID)
                {
                    alreadyConnected = true;
                    break;
                }
            }
            clients.Remove(user);
        }
    }

    private void OnClientJoined(User user)
    {
        clients.Add(user);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

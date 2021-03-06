﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using System;

public class OculusAuthentifier : MonoBehaviour
{
    [Header("Reference Settings")]
    public GameObject LogWall;
    public GameObject networkAutoSelector;
    [Header("Monitoring")]
    [ReadOnly]
    public string _OculusId;
    public static string OculusId;
    private void Awake()
    {
        Core.AsyncInitialize().OnComplete(OnCoreInit);
    }

    private void OnCoreInit(Message<PlatformInitialize> message)
    {
        Entitlements.IsUserEntitledToApplication().OnComplete(UserEntitled);
        Users.GetLoggedInUser().OnComplete(OnUserInfo);
    }

    private void UserEntitled(Message msg)
    {
        if (msg.IsError)
        {
            Debug.Log("You are not entitled to use this app");
        }
        else
        {
            Debug.Log("You are entitled to use this app");
        }
    }
    private void OnUserInfo(Message<User> msg)
    {
        _OculusId = msg.Data.OculusID;
        OculusId = msg.Data.OculusID;
        Debug.Log("LOGGED IN AS "+msg.Data.OculusID);
        LogWall.SetActive(false);
        networkAutoSelector.SetActive(true);
    }
}

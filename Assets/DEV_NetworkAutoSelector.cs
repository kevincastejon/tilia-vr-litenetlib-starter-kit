using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEV_NetworkAutoSelector : MonoBehaviour
{
    public Activable CreateButton;
    public Activable JoinButton;
    public bool isServer;
    private void Awake()
    {
        if (isServer)
        {
            CreateButton.Activate();
        }
        else
        {
            JoinButton.Activate();
        }
    }
}

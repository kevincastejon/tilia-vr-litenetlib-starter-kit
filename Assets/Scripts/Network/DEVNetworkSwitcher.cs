using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEVNetworkSwitcher : MonoBehaviour
{
    
    static public bool isServer;
    public bool _isServer;
    public GameObject server;
    public GameObject client;
    private void Start()
    {
        if (_isServer)
        {
            isServer = true;
            Destroy(client);
            server.SetActive(true);
        }
        else
        {
            isServer = false;
            Destroy(server);
            client.SetActive(true);
        }
    }
}

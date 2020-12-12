using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEVNetworkSwitcher : MonoBehaviour
{
    
    static public bool isServer;
    static public bool showLagLogs;
    public bool _isServer;
    public GameObject server;
    public GameObject client;
    public bool _showLagLogs;
    private void Awake()
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
        if (_showLagLogs)
        {
            showLagLogs = _showLagLogs;
        }
    }
}

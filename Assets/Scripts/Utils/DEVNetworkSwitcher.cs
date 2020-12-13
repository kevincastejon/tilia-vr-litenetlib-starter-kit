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
    public GameObject TiliaObjects;
    public bool _showLagLogs;
    private void Awake()
    {
        //if (_isServer)
        //{
        //    isServer = true;
        //    Destroy(client);
        //    server.SetActive(true);
        //}
        //else
        //{
        //    isServer = false;
        //    Destroy(server);
        //    client.SetActive(true);
        //}
        if (_showLagLogs)
        {
            showLagLogs = _showLagLogs;
        }
    }
    public void SetupServer()
    {
        Debug.Log("SETUP SERVER");
        isServer = true;
        Destroy(client);
        server.SetActive(true);
        TiliaObjects.SetActive(true);
    }

    public void SetupClient()
    {
        Debug.Log("SETUP CLIENT");
        isServer = false;
        Destroy(server);
        client.SetActive(true);
        TiliaObjects.SetActive(true);
    }
}

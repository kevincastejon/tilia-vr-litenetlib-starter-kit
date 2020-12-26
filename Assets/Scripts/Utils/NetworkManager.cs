using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    
    static public bool isServer;
    static public bool showLagLogs;
    [Header("Reference Settings")]
    public GameObject server;
    public GameObject client;
    public GameObject TiliaObjects;
    [Header("Dev Settings")]
    public bool _showLagLogs;
    private void Awake()
    {
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

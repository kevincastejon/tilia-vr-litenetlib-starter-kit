using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEVNetworkSwitcher : MonoBehaviour
{
    public bool isServer;
    public GameObject server;
    public GameObject client;
    private void Start()
    {
        if (isServer)
        {
            Destroy(client);
            server.SetActive(true);
        }
        else
        {
            Destroy(server);
            client.SetActive(true);
        }
    }
}

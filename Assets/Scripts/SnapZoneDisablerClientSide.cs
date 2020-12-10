using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapZoneDisablerClientSide : MonoBehaviour
{
    private void Awake()
    {
        if (!DEVNetworkSwitcher.isServer)
        {
            Destroy(gameObject);
        }
    }
}

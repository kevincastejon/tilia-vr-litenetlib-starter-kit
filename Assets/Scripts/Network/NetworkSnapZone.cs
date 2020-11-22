using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.SnapZone;
using UnityEngine;


[RequireComponent(typeof(SnapZoneFacade))]
public class NetworkSnapZone : MonoBehaviour
{
    private SnapZoneFacade snapZone;
    private void Start()
    {
        snapZone = GetComponent<SnapZoneFacade>();
        if (!DEVNetworkSwitcher.isServer)
        {
            Destroy(gameObject);
        }
    }
}

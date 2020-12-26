using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.SnapZone;
using UnityEngine;

[RequireComponent(typeof(SnapZoneFacade))]
public class NetworkedSnapZone : MonoBehaviour
{
    SnapZoneFacade snapZone;
    private void Awake()
    {
        snapZone = GetComponent<SnapZoneFacade>();
        if (!NetworkManager.isServer)
        {
            Destroy(gameObject);
        }
        snapZone.Snapped.AddListener(OnSnap);
        snapZone.Unsnapped.AddListener(OnUnsnap);
    }

    private void OnSnap(GameObject obj)
    {
        Entity ent = obj.GetComponent<Entity>();
        if (ent)
        {
            ent.snapZone = snapZone;
        }
    }

    private void OnUnsnap(GameObject obj)
    {
        Entity ent = obj.GetComponent<Entity>();
        if (ent)
        {
            ent.snapZone = null;
        }
    }
}

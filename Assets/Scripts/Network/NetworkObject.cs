using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using Tilia.Interactions.SnapZone;
using UnityEngine;
using UnityEngine.Events;

//[RequireComponent(typeof(InteractableFacade))]
//[RequireComponent(typeof(Rigidbody))]

public class NetworkObject : MonoBehaviour
{
    public EntityType type;
    public Rigidbody body;
    [ReadOnly]
    public int id;
    [ReadOnly]
    public bool grabbed;
    [ReadOnly]
    public bool leftHand;
    [ReadOnly]
    public int lastOwnerId;
    [ReadOnly]
    public SnapZoneFacade snapContainer;
    [HideInInspector]
    public Vector3 bufferVelocity;
    [HideInInspector]
    public Vector3 bufferAngularVelocity;
    [HideInInspector]
    public bool kinematicInitValue;

    private void Awake()
    {
        if (DEVNetworkSwitcher.isServer)
        {
            id = GetInstanceID();
            if (body)
            {
                kinematicInitValue = body.isKinematic;
            }
        }
        else
        {
            if (body)
            {
                body.isKinematic = true;
            }
        }
    }
}

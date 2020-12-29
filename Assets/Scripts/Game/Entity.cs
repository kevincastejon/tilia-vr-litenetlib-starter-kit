using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using Tilia.Interactions.SnapZone;
using UnityEngine;

public enum EntityType
{
    Bullet = 0,
    Ball = 1,
    Pin = 2,
    Gun = 3,
}

public class Entity : MonoBehaviour
{
    [Header("Entity Type")]
    public EntityType type;
    //[Header("Grabbable from your hand by remote player")]         //Hard to implement... Let's forbid it for now
    //public bool isGrabbableFromHand;
    [Header("Network Settings")]
    public int priority = 1;
    public int priorityAccumulator = 0;
    [Header("Reference Settings (transformTarget is MANDATORY)")]
    public Transform transformTarget;
    public InteractableFacade interactable;
    public Rigidbody body;
    [HideInInspector]
    public bool initialIsKinematic;
    [Header("Monitoring")]
    [ReadOnly]
    public int id;
    [ReadOnly]
    public int ownerId=-1;
    [ReadOnly]
    public SnapZoneFacade snapZone;
    private void Awake()
    {
        if (NetworkManager.isServer)
        {
            GameManagerServer.instance.AddEntity(this);
            if (body)
            {
                initialIsKinematic = body.isKinematic;
            }
            if (interactable)
            {
                interactable.Grabbed.AddListener(OnGrab);
                interactable.Ungrabbed.AddListener(OnUngrab);
            }
        }
        else
        {
            GameManagerClient.instance.AddEntity(this);
            if (body)
            {
                body.isKinematic = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (body)
        {
            if (body.velocity.Equals(Vector3.zero) && body.angularVelocity.Equals(Vector3.zero))
            {
                priority = 1;
            }
            else
            {
                priority = 100;
            }
        }
        priorityAccumulator += priority;
    }

    private void OnGrab(InteractorFacade interactorFacade)
    {
        priority = 1000000;
    }

    private void OnUngrab(InteractorFacade arg0)
    {
        if (body)
        {
            if (body.velocity.Equals(Vector3.zero) && body.angularVelocity.Equals(Vector3.zero))
            {
                priority = 1;
            }
            else
            {
                priority = 100;
            }
        }
        else
        {
            priority = 1;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.isServer)
        {
            GameManagerServer.instance.RemoveEntity(this);
        }
        else
        {
            GameManagerClient.instance.RemoveEntity(this);
        }
    }
}

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
    //[Header("Network Settings")]
    //public float priority = 0f;
    //public float priorityAccumulator = 0f;
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
    public int ownerId = -1;
    [ReadOnly]
    public SnapZoneFacade snapZone;
    [ReadOnly]
    public float lastSerialization;
    [ReadOnly]
    public EntityState stateA;
    [ReadOnly]
    public int sequenceA = -1;
    [ReadOnly]
    public EntityState stateB;
    [ReadOnly]
    public int sequenceB;
    [ReadOnly]
    public LiteRingBuffer<EntityState> stateBuffer;

    public void PushState(EntityState es, int sequence)
    {
        if (stateA == null && stateB == null)
        {
            stateA = es;
            sequenceA = sequence;
        }
        else if (stateB == null)
        {
            stateB = es;
            sequenceB = sequence;
        }
        else
        {
            stateA = stateB;
            sequenceA = sequenceB;
            stateB = es;
            sequenceB = sequence;
        }
    }

    public void Lerp(float _t)
    {
        if (stateB == null)
        {
            return;
        }
        float t = _t / (sequenceB - sequenceA);
        transformTarget.position = Vector3.Lerp(stateA.Position, stateB.Position, t);
        transformTarget.rotation = Quaternion.Lerp(stateA.Rotation, stateB.Rotation, t);
        ownerId = stateA.Owner;
        if (interactable)
        {
            if (ownerId != -1)
            {
                interactable.DisableGrab();
            }
            else
            {
                interactable.EnableGrab();
            }
        }
    }

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
        //if (body)
        //{
        //    if (body.velocity.Equals(Vector3.zero) && body.angularVelocity.Equals(Vector3.zero))
        //    {
        //        priority = 0f;
        //    }
        //    else
        //    {
        //        priority = 100f;
        //    }
        //}
        //priorityAccumulator += priority;
        lastSerialization += 1f;
    }

    private void OnGrab(InteractorFacade interactorFacade)
    {
        //priority = 1000000f;
    }

    private void OnUngrab(InteractorFacade arg0)
    {
        //if (body)
        //{
        //    if (body.velocity.Equals(Vector3.zero) && body.angularVelocity.Equals(Vector3.zero))
        //    {
        //        priority = 0f;
        //    }
        //    else
        //    {
        //        priority = 100f;
        //    }
        //}
        //else
        //{
        //    priority = 0f;
        //}
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

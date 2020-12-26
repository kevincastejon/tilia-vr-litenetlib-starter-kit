using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
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
    [Header("Monitoring")]
    [ReadOnly]
    public int id;
    [ReadOnly]
    public int ownerId=-1;
    [ReadOnly]
    public SnapZoneFacade snapZone;
    [Header("Entity type")]
    public EntityType type;
    //[Header("Grabbable from your hand by remote player")]         //Hard to implement... Let's forbid it for now
    //public bool isGrabbableFromHand;
    [Header("Reference settings (transformTarget is MANDATORY)")]
    public Transform transformTarget;
    public InteractableFacade interactable;
    public Rigidbody body;
    [HideInInspector]
    public bool initialIsKinematic;
    private void Awake()
    {
        if (NetworkManager.isServer)
        {
            GameManagerServer.instance.AddEntity(this);
            if (body)
            {
                initialIsKinematic = body.isKinematic;
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

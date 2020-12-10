using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
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
    [ReadOnly]
    public int id;
    public EntityType type;
    public Transform transformTarget;
    public InteractableFacade interactable;
    public Rigidbody body;
    private void Awake()
    {
        if (DEVNetworkSwitcher.isServer)
        {
            GameManagerServer.instance.AddEntity(this);
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
        if (DEVNetworkSwitcher.isServer)
        {
            GameManagerServer.instance.RemoveEntity(this);
        }
        else
        {
            GameManagerClient.instance.RemoveEntity(this);
        }
    }
}

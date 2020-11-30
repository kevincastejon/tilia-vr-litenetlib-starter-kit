using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EntityTypeSettings
{
    public GameObject prefab;
    public int max;
    public bool removeFirstEntity;
}
[Serializable]
public class NetworkObjectEvent : UnityEvent<NetworkObject> { }

public class NetworkManager : MonoBehaviour
{
    public List<EntityTypeSettings> entityTypesSettings = new List<EntityTypeSettings>();
    private static NetworkManager _instance;
    
    static public NetworkManager GetInstance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    //public EntityState[] GetEntityStates()
    //{
    //    EntityState[] entityStates = new EntityState[objects.Count];
    //    for (int i = 0; i < objects.Count; i++)
    //    {
    //        NetworkObject b = objects[i];
    //        entityStates[i] = new EntityState()
    //        {
    //            Id = b.id,
    //            Type = (byte)b.type,
    //            Position = b.transform.position,
    //            Rotation = b.transform.rotation,
    //        };
    //    }
    //    return (entityStates);
    //}
}

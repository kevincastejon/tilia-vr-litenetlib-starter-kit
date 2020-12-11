﻿using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;

[Serializable]
public class InstanceCountLimit
{
    public int max = -1;
    public bool destroyFirst = false;
}
public class GameManagerServer : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    [Header("Reference Settings")]
    public LocalAvatar localAvatar;
    public GameServer server;
    public List<InstanceCountLimit> entityTypeInstanceLimits = new List<InstanceCountLimit>();
    [Header("Monitoring")]
    [ReadOnly]
    public int Sequence;
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    public List<Entity> entities = new List<Entity>();
    private float timerMax = 2 / 60f;
    private float timer = 0f;
    [HideInInspector]
    public static GameManagerServer instance;

    private void Awake()
    {
        instance = this;
        localAvatar.id = -128;
        localAvatar.OnShoot.AddListener(ShootBullet);
    }

    public void AddEntity(Entity ent)
    {
        int max = entityTypeInstanceLimits[(int)ent.type].max;
        int currentCount = GetEntityTypeCount(ent.type);
        bool destroyFirst = entityTypeInstanceLimits[(int)ent.type].destroyFirst;
        if (max == -1 || currentCount < max)
        {
            ent.id = ent.GetInstanceID();
            entities.Add(ent);
            if (ent.interactable != null)
            {
                ent.interactable.Grabbed.AddListener((InteractorFacade ifc) => OnLocalGrab(ent));
                ent.interactable.Ungrabbed.AddListener((InteractorFacade ifc) => OnLocalUngrab(ent));
            }
        }
        else
        {
            Entity destroyingEnt;
            if (destroyFirst)
            {
                destroyingEnt = entities.Find(x => x.type == ent.type);
                entities.Remove(destroyingEnt);
                if (destroyingEnt.interactable != null)
                {
                    destroyingEnt.interactable.Grabbed.RemoveAllListeners();
                    destroyingEnt.interactable.Ungrabbed.RemoveAllListeners();
                }
                ent.id = ent.GetInstanceID();
                entities.Add(ent);
                if (ent.interactable != null)
                {
                    ent.interactable.Grabbed.AddListener((InteractorFacade ifc) => OnLocalGrab(ent));
                    ent.interactable.Ungrabbed.AddListener((InteractorFacade ifc) => OnLocalUngrab(ent));
                }
            }
            else
            {
                destroyingEnt = ent;
            }
            Destroy(destroyingEnt.gameObject);
        }
    }
    public void RemoveEntity(Entity ent)
    {
        if (entities.Contains(ent))
        {
            entities.Remove(ent);
            if (ent.interactable != null)
            {
                ent.interactable.Grabbed.RemoveAllListeners();
                ent.interactable.Ungrabbed.RemoveAllListeners();
            }
        }
    }

    private void OnLocalGrab(Entity ent)
    {

    }

    private void OnLocalUngrab(Entity ent)
    {

    }

    private int GetEntityTypeCount(EntityType type)
    {
        int count = 0;
        foreach (Entity entity in entities)
        {
            if (entity.type == type)
            {
                count++;
            }
        }
        return count;
    }

    private void FixedUpdate()
    {
        float newTimer = timer + Time.fixedDeltaTime;
        bool isLastFrame = newTimer > timerMax;
        LerpPlayers(timer / timerMax, isLastFrame);
        timer = newTimer;
        if (isLastFrame)
        {
            timer -= timerMax;
            SendState();
        }
    }
    public void OnPlayerConnected(int peerID)
    {
        Player newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
        newPlayer.nameOrientationTarget = localAvatar.headAlias;
        newPlayer.id = peerID;
        players[peerID] = newPlayer;
        InitMessage im = new InitMessage()
        {
            OwnId = peerID
        };
        server.SendInitMessage(im, peerID);
    }

    public void OnPlayerDisconnected(int peerID)
    {
        Player disconnectedPlayer = players[peerID];
        players.Remove(peerID);
        if (disconnectedPlayer.leftGrabbed)
        {

        }
        if (disconnectedPlayer.rightGrabbed)
        {

        }
        Destroy(disconnectedPlayer.gameObject);
    }

    public void OnPlayerInput(int playerId, PlayerInput pi)
    {
        Player p = players[playerId];
        p.AddStateToBuffer(pi.Clone());
    }

    public PlayerState[] GetPlayersStates(int excludedPlayerId)
    {
        PlayerState[] playerStates = new PlayerState[players.Count];
        int playerStateCount = 0;
        foreach (KeyValuePair<int, Player> entry in players)
        {
            Player p = entry.Value;
            if (p.id != excludedPlayerId)
            {
                playerStates[playerStateCount] = new PlayerState()
                {
                    Id = p.id,
                    HeadPosition = p.headAlias.transform.position,
                    HeadRotation = p.headAlias.transform.rotation,
                    LeftHandPosition = p.leftHandAlias.transform.position,
                    LeftHandRotation = p.leftHandAlias.transform.rotation,
                    RightHandPosition = p.rightHandAlias.transform.position,
                    RightHandRotation = p.rightHandAlias.transform.rotation,
                    LeftPointer = p.LeftPointer,
                    RightPointer = p.RightPointer,
                };
                playerStateCount++;
            }
        }
        playerStates[playerStateCount] = new PlayerState()
        {
            Id = -128,     //Server id is always -128
            HeadPosition = localAvatar.headAlias.transform.position,
            HeadRotation = localAvatar.headAlias.transform.rotation,
            LeftHandPosition = localAvatar.leftHandAlias.transform.position,
            LeftHandRotation = localAvatar.leftHandAlias.transform.rotation,
            RightHandPosition = localAvatar.rightHandAlias.transform.position,
            RightHandRotation = localAvatar.rightHandAlias.transform.rotation,
            LeftPointer = localAvatar.leftPointer,
            RightPointer = localAvatar.rightPointer,
        };
        return playerStates;
    }

    public EntityState[] GetEntitiesStates()
    {
        EntityState[] entityStates = new EntityState[entities.Count];
        int entityStateCount = 0;
        foreach (Entity ent in entities)
        {
            entityStates[entityStateCount] = new EntityState()
            {
                Id = ent.id,
                Type = (byte)ent.type,
                Position = ent.transform.position,
                Rotation = ent.transform.rotation,
                Owner = ent.ownerId,
            };
            entityStateCount++;
        }
        return entityStates;
    }

    private void SendState()
    {
        foreach (KeyValuePair<int, Player> entry in players)
        {
            PlayerState[] playerStates = GetPlayersStates(entry.Key);
            EntityState[] entityStates = GetEntitiesStates();
            server.SendWorldState(
                new StateMessage()
                {
                    Sequence = Sequence,
                    Entities = entityStates,
                    Players = playerStates
                },
                entry.Value.id
            );
        }
        Sequence++;
    }

    private void LerpPlayers(float t, bool isLastFrame)
    {
        for (int i = 0; i < players.Count; i++)
        {
            Player p = players[i];
            if (p.nameOrientationTarget)
            {
                p.playerName.transform.rotation = Quaternion.LookRotation(p.playerName.transform.position - p.nameOrientationTarget.transform.position);
            }

            if (p.inputBuffer.Count < 2)
            {
                Debug.Log("NOT ENOUGTH DATA RECEIVED FROM PLAYER " + p.id);
                return;
            }
            var dataA = p.inputBuffer[0];
            var dataB = p.inputBuffer[1];

            p.headAlias.transform.position = Vector3.Lerp(dataA.HeadPosition, dataB.HeadPosition, t);
            p.headAlias.transform.rotation = Quaternion.Lerp(dataA.HeadRotation, dataB.HeadRotation, t);
            p.leftHandAlias.transform.position = Vector3.Lerp(dataA.LeftHandPosition, dataB.LeftHandPosition, t);
            p.leftHandAlias.transform.rotation = Quaternion.Lerp(dataA.LeftHandRotation, dataB.LeftHandRotation, t);
            p.rightHandAlias.transform.position = Vector3.Lerp(dataA.RightHandPosition, dataB.RightHandPosition, t);
            p.rightHandAlias.transform.rotation = Quaternion.Lerp(dataA.RightHandRotation, dataB.RightHandRotation, t);
            if (dataA.LeftGrabId != dataB.LeftGrabId && dataA.LeftGrabId != 0)
            {
                //Ungrab left
                Entity ent = entities.Find(x => x.id == dataA.LeftGrabId);
                p.leftGrabbed = null;
                ent.ownerId = -1;
                DisableCollisionsOnGrab comp = ent.GetComponent<DisableCollisionsOnGrab>();
                if (comp)
                {
                    comp.EnableColliders();
                }
                if (ent.body)
                {
                    ent.body.isKinematic = ent.initialIsKinematic;
                    ent.body.velocity = dataA.LeftGrabVelocity;
                    ent.body.angularVelocity = dataA.LeftGrabAngularVelocity;
                }

            }
            else if (dataA.LeftGrabId != 0)
            {
                //Grab left
                Entity ent = entities.Find(x => x.id == dataA.LeftGrabId);
                if (ent.snapZone)
                {
                    ent.snapZone.Unsnap();
                }
                if (ent.body)
                {
                    ent.body.isKinematic = true;
                }
                p.leftGrabbed = ent;
                ent.ownerId = p.id;
                DisableCollisionsOnGrab comp = ent.GetComponent<DisableCollisionsOnGrab>();
                if (comp && !comp.collidesOnGrab)
                {
                    comp.DisableColliders();
                }
                ent.transformTarget.position = Vector3.Lerp(dataA.LeftGrabPosition, dataB.LeftGrabPosition, t);
                ent.transformTarget.rotation = Quaternion.Lerp(dataA.LeftGrabRotation, dataB.LeftGrabRotation, t);
            }

            if (dataA.RightGrabId != dataB.RightGrabId && dataA.RightGrabId != 0)
            {
                //Ungrab right
                Entity ent = entities.Find(x => x.id == dataA.RightGrabId);
                p.rightGrabbed = null;
                ent.ownerId = -1;
                DisableCollisionsOnGrab comp = ent.GetComponent<DisableCollisionsOnGrab>();
                if (comp)
                {
                    comp.EnableColliders();
                }
                if (ent.body)
                {
                    ent.body.isKinematic = ent.initialIsKinematic;
                    ent.body.velocity = dataA.RightGrabVelocity;
                    ent.body.angularVelocity = dataA.RightGrabAngularVelocity;
                }

            }
            else if (dataA.RightGrabId != 0)
            {
                //Grab right
                Entity ent = entities.Find(x => x.id == dataA.RightGrabId);
                if (ent.snapZone)
                {
                    ent.snapZone.Unsnap();
                }
                if (ent.body)
                {
                    ent.body.isKinematic = true;
                }
                p.rightGrabbed = ent;
                ent.ownerId = p.id;
                DisableCollisionsOnGrab comp = ent.GetComponent<DisableCollisionsOnGrab>();
                if (comp && !comp.collidesOnGrab)
                {
                    comp.DisableColliders();
                }
                ent.transformTarget.position = Vector3.Lerp(dataA.RightGrabPosition, dataB.RightGrabPosition, t);
                ent.transformTarget.rotation = Quaternion.Lerp(dataA.RightGrabRotation, dataB.RightGrabRotation, t);
            }
            if (isLastFrame)
            {
                p.inputBuffer.RemoveFromStart(1);
                p.inputBufferLength = p.inputBuffer.Count;
            }
        }
    }

    public void ShootBullet(Gun gun)
    {
        GameObject bullet = Instantiate(bulletPrefab, gun.spawnPoint.position, gun.spawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 5, ForceMode.Impulse);
    }
}

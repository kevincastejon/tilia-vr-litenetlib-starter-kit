using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;

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
        localAvatar.id = -1;
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
            }
            else
            {
                destroyingEnt = ent;
            }
            entities.Remove(destroyingEnt);
            if (destroyingEnt.interactable != null)
            {
                destroyingEnt.interactable.Grabbed.RemoveAllListeners();
                destroyingEnt.interactable.Ungrabbed.RemoveAllListeners();
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
        newPlayer.SetNameOrientationTarget(localAvatar.headAlias);
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
            Id = -1,     //Server id is always -1
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
            p.UpdatePosition(t, isLastFrame);
        }
    }

    public void ShootBullet(Gun gun)
    {
        GameObject bullet = Instantiate(bulletPrefab, gun.spawnPoint.position, gun.spawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 5, ForceMode.Impulse);
    }
}

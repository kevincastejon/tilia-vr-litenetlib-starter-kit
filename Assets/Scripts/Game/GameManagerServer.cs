using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerServer : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject playerPrefab;
    [Header("Reference Settings")]
    public LocalAvatar localAvatar;
    public Server server;
    public ColoredCube coloredCube;
    public EntitiesSettings entitiesSettings;
    [Header("Monitoring")]
    [ReadOnly]
    public int Sequence;
    public List<Entity> entities = new List<Entity>();
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private int numConnectedPlayers;
    private float timerMax = 4 / 60f;
    private float timer = 0f;
    [HideInInspector]
    public static GameManagerServer instance;

    private void Awake()
    {
        instance = this;
        localAvatar.id = -128;
        localAvatar.OnShoot.AddListener(ShootBullet);
        localAvatar.TransportTo(new Vector3(0f, 10f, -12.5f));
        Debug.Log("GAMEMANAGER SERVER AWAKED");
    }

    public void AddEntity(Entity ent)
    {
        int max = entitiesSettings.settings[(int)ent.type].maxInstance;
        int currentCount = GetEntityTypeCount(ent.type);
        bool destroyFirst = entitiesSettings.settings[(int)ent.type].destroyFirst;
        if (max == -1 || currentCount < max)
        {
            ent.id = ent.GetInstanceID();
            entities.Add(ent);
            server.SendImportantMessage(new EntityAddMessage()
            {
                Id = ent.id,
                Owner = ent.ownerId,
                Position = ent.transform.position,
                Rotation = ent.transform.rotation,
                Type = (byte)ent.type
            });
        }
        else
        {
            Entity destroyingEnt;
            if (destroyFirst)
            {
                destroyingEnt = entities.Find(x => x.type == ent.type);
                entities.Remove(destroyingEnt);
                server.SendImportantMessage(new EntityRemoveMessage()
                {
                    Id = destroyingEnt.id
                });
                if (destroyingEnt.interactable != null)
                {
                    destroyingEnt.interactable.Grabbed.RemoveAllListeners();
                    destroyingEnt.interactable.Ungrabbed.RemoveAllListeners();
                }
                ent.id = ent.GetInstanceID();
                entities.Add(ent);
                server.SendImportantMessage(new EntityAddMessage()
                {
                    Id = ent.id,
                    Owner = ent.ownerId,
                    Position = ent.transform.position,
                    Rotation = ent.transform.rotation,
                    Type = (byte)ent.type
                });
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
            server.SendImportantMessage(new EntityRemoveMessage()
            {
                Id = ent.id
            });
        }
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
    }
    public void OnPlayerInitInfo(int peerID, PlayerInitInfo inf)
    {
        Player p = players[peerID];
        p.connected = true;
        p.oculusId = inf.OculusID;
        p.playerName.text = inf.OculusID;
        p.OnShoot.AddListener(ShootBullet);
        p.headAlias.transform.position = inf.HeadPosition;
        p.headAlias.transform.rotation = inf.HeadRotation;
        p.leftHandAlias.transform.position = inf.LeftHandPosition;
        p.leftHandAlias.transform.rotation = inf.LeftHandRotation;
        p.rightHandAlias.transform.position = inf.RightHandPosition;
        p.rightHandAlias.transform.rotation = inf.RightHandRotation;
        p.LeftPointer = inf.LeftPointer;
        p.RightPointer = inf.RightPointer;
        InitMessage im = new InitMessage()
        {
            OwnId = peerID,
            Entities = GetEntitiesAddMessage(),
            Players = GetPlayersAddMessage(peerID),
            ColoredCube = coloredCube.currentColor
        };
        server.SendImportantMessage(im, peerID);
        server.SendImportantMessage(new PlayerAddMessage()
        {
            Id = peerID,
            OculusId = inf.OculusID,
            HeadPosition = inf.HeadPosition,
            HeadRotation = inf.HeadRotation,
            LeftHandPosition = inf.LeftHandPosition,
            LeftHandRotation = inf.LeftHandRotation,
            RightHandPosition = inf.RightHandPosition,
            RightHandRotation = inf.RightHandRotation,
            LeftPointer = inf.LeftPointer,
            RightPointer = inf.RightPointer,
        }, peerID, true);
        numConnectedPlayers++;
    }

    public void OnPlayerDisconnected(int peerID)
    {
        Player disconnectedPlayer = players[peerID];
        players.Remove(peerID);
        if (disconnectedPlayer.leftGrabbed)
        {
            Entity ent = disconnectedPlayer.leftGrabbed;
            ent.ownerId = -1;
            ent.interactable.EnableGrab();
            DisableCollisionsOnGrab dcog = ent.GetComponent<DisableCollisionsOnGrab>();
            if (dcog)
            {
                dcog.EnableColliders();
            }
            if (ent.body)
            {
                ent.body.isKinematic = ent.initialIsKinematic;
                if (disconnectedPlayer.inputBufferLength > 1)
                {
                    ent.body.velocity = disconnectedPlayer.inputBuffer[0].LeftGrabVelocity;
                    ent.body.angularVelocity = disconnectedPlayer.inputBuffer[0].LeftGrabAngularVelocity;
                }
            }
        }
        if (disconnectedPlayer.rightGrabbed)
        {
            Entity ent = disconnectedPlayer.rightGrabbed;
            ent.ownerId = -1;
            ent.interactable.EnableGrab();
            DisableCollisionsOnGrab dcog = ent.GetComponent<DisableCollisionsOnGrab>();
            if (dcog)
            {
                dcog.EnableColliders();
            }
            if (ent.body)
            {
                ent.body.isKinematic = ent.initialIsKinematic;
                if (disconnectedPlayer.inputBufferLength > 1)
                {
                    ent.body.velocity = disconnectedPlayer.inputBuffer[0].LeftGrabVelocity;
                    ent.body.angularVelocity = disconnectedPlayer.inputBuffer[0].LeftGrabAngularVelocity;
                }
            }
        }
        Destroy(disconnectedPlayer.gameObject);
        server.SendImportantMessage(new PlayerRemoveMessage() { Id = peerID });
        if (!disconnectedPlayer.connected)
        {
            numConnectedPlayers--;
        }
    }

    public void OnPlayerInput(int playerId, PlayerInput pi)
    {
        Player p = players[playerId];
        p.AddStateToBuffer(pi.Clone());
    }

    public PlayerState[] GetPlayersStates(int excludedPlayerId)
    {
        PlayerState[] playerStates = new PlayerState[numConnectedPlayers];
        int playerStateCount = 0;
        foreach (KeyValuePair<int, Player> entry in players)
        {
            Player p = entry.Value;
            if (!p.connected)
            {
                continue;
            }
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
                    LeftPointer = p.leftGrabbed ? false : p.LeftPointer,
                    RightPointer = p.rightGrabbed ? false : p.RightPointer,
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
            LeftPointer = localAvatar.leftGrabbed ? false : localAvatar.leftPointer,
            RightPointer = localAvatar.rightGrabbed ? false : localAvatar.rightPointer,
        };
        return playerStates;
    }
    public PlayerAddMessage[] GetPlayersAddMessage(int excludedPlayerId)
    {
        PlayerAddMessage[] playerStates = new PlayerAddMessage[players.Count];
        int playerStateCount = 0;
        foreach (KeyValuePair<int, Player> entry in players)
        {
            Player p = entry.Value;
            if (p.id != excludedPlayerId)
            {
                playerStates[playerStateCount] = new PlayerAddMessage()
                {
                    Id = p.id,
                    OculusId = p.oculusId,
                    HeadPosition = p.headAlias.transform.position,
                    HeadRotation = p.headAlias.transform.rotation,
                    LeftHandPosition = p.leftHandAlias.transform.position,
                    LeftHandRotation = p.leftHandAlias.transform.rotation,
                    RightHandPosition = p.rightHandAlias.transform.position,
                    RightHandRotation = p.rightHandAlias.transform.rotation,
                    LeftPointer = p.leftGrabbed ? false : p.LeftPointer,
                    RightPointer = p.rightGrabbed ? false : p.RightPointer,
                };
                playerStateCount++;
            }
        }
        playerStates[playerStateCount] = new PlayerAddMessage()
        {
            Id = -128,     //Server id is always -128
            OculusId = OculusAuthentifier.OculusId,
            HeadPosition = localAvatar.headAlias.transform.position,
            HeadRotation = localAvatar.headAlias.transform.rotation,
            LeftHandPosition = localAvatar.leftHandAlias.transform.position,
            LeftHandRotation = localAvatar.leftHandAlias.transform.rotation,
            RightHandPosition = localAvatar.rightHandAlias.transform.position,
            RightHandRotation = localAvatar.rightHandAlias.transform.rotation,
            LeftPointer = localAvatar.leftGrabbed ? false : localAvatar.leftPointer,
            RightPointer = localAvatar.rightGrabbed ? false : localAvatar.rightPointer,
        };
        return playerStates;
    }

    public EntityState[] GetEntitiesStates()
    {
        int max = entitiesSettings.maxEntitiesStateSend < entities.Count ? entitiesSettings.maxEntitiesStateSend : entities.Count;
        EntityState[] entityStates = new EntityState[max];
        int entityStateCount = 0;
        entities.Sort((Entity a, Entity b) => { return a.lastSerialization > b.lastSerialization ? -1 : 1; });
        foreach (Entity ent in entities)
        {
            if (entityStateCount >= max)
            {
                break;
            }
            entityStates[entityStateCount] = new EntityState()
            {
                Id = ent.id,
                Type = (byte)ent.type,
                Position = ent.transform.position,
                Rotation = ent.transform.rotation,
                Owner = ent.ownerId,
            };
            //ent.priorityAccumulator = 0f;
            ent.lastSerialization = 0f;
            entityStateCount++;
        }
        return entityStates;
    }
    public EntityAddMessage[] GetEntitiesAddMessage()
    {
        EntityAddMessage[] entityStates = new EntityAddMessage[entities.Count];
        int entityStateCount = 0;
        foreach (Entity ent in entities)
        {
            entityStates[entityStateCount] = new EntityAddMessage()
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
            if (!entry.Value.connected)
            {
                continue;
            }
            PlayerState[] playerStates = GetPlayersStates(entry.Key);
            EntityState[] entityStates = GetEntitiesStates();
            server.SendFastMessage(
                new StateMessage()
                {
                    Sequence = Sequence,
                    Entities = entityStates,
                    Players = playerStates,
                    ColoredCube = coloredCube.currentColor,
                },
                entry.Value.id
            );
        }
        Sequence++;
    }

    private void LerpPlayers(float t, bool isLastFrame)
    {
        foreach (KeyValuePair<int, Player> entry in players)
        {
            Player p = entry.Value;
            p.playerName.transform.rotation = Quaternion.LookRotation(p.playerName.transform.position - p.nameOrientationTarget.transform.position);


            if (p.inputBuffer.Count < 2)
            {
                if (NetworkManager.showLagLogs)
                {
                    Debug.Log("NOT ENOUGTH DATA RECEIVED FROM PLAYER " + p.id);
                }
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
            p.LeftTrigger = dataA.LeftTrigger;
            p.RightTrigger = dataA.RightTrigger;
            if (dataA.LeftGrabId != dataB.LeftGrabId && dataA.LeftGrabId != 0)
            {
                //Ungrab left
                Entity ent = entities.Find(x => x.id == dataA.LeftGrabId);
                p.leftGrabbed = null;
                ent.ownerId = -1;
                ent.interactable.EnableGrab();
                DisableCollisionsOnGrab dcog = ent.GetComponent<DisableCollisionsOnGrab>();
                if (dcog)
                {
                    dcog.EnableColliders();
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
                ent.interactable.DisableGrab();
                DisableCollisionsOnGrab dcog = ent.GetComponent<DisableCollisionsOnGrab>();
                if (dcog && !dcog.collidesOnGrab)
                {
                    dcog.DisableColliders();
                }
                if (ent.body)
                {
                    ent.body.isKinematic = true;
                }
                p.leftGrabbed = ent;
                ent.ownerId = p.id;
                ent.transformTarget.position = Vector3.Lerp(dataA.LeftGrabPosition, dataB.LeftGrabPosition, t);
                ent.transformTarget.rotation = Quaternion.Lerp(dataA.LeftGrabRotation, dataB.LeftGrabRotation, t);
            }

            if (dataA.RightGrabId != dataB.RightGrabId && dataA.RightGrabId != 0)
            {
                //Ungrab right
                Entity ent = entities.Find(x => x.id == dataA.RightGrabId);
                p.rightGrabbed = null;
                ent.ownerId = -1;
                ent.interactable.EnableGrab();
                DisableCollisionsOnGrab dcog = ent.GetComponent<DisableCollisionsOnGrab>();
                if (dcog)
                {
                    dcog.EnableColliders();
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
                ent.interactable.DisableGrab();
                DisableCollisionsOnGrab dcog = ent.GetComponent<DisableCollisionsOnGrab>();
                if (dcog && !dcog.collidesOnGrab)
                {
                    dcog.DisableColliders();
                }
                if (ent.body)
                {
                    ent.body.isKinematic = true;
                }
                p.rightGrabbed = ent;
                ent.ownerId = p.id;
                ent.transformTarget.position = Vector3.Lerp(dataA.RightGrabPosition, dataB.RightGrabPosition, t);
                ent.transformTarget.rotation = Quaternion.Lerp(dataA.RightGrabRotation, dataB.RightGrabRotation, t);
            }
            p.LeftPointer = p.leftGrabbed != null ? false : dataA.LeftPointer;
            p.RightPointer = p.rightGrabbed != null ? false : dataA.RightPointer;

            if (isLastFrame)
            {
                p.inputBuffer.RemoveFromStart(1);
                p.inputBufferLength = p.inputBuffer.Count;
            }
        }
    }

    public void ShootBullet(Gun gun)
    {
        GameObject bullet = Instantiate(entitiesSettings.settings[(int)EntityType.Bullet].prefab, gun.spawnPoint.position, gun.spawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 5, ForceMode.Impulse);
    }
}

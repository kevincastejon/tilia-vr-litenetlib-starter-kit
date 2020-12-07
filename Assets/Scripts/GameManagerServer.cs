using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using UnityEngine;

public class GameManagerServer : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    [Header("Reference Settings")]
    public LocalAvatar localAvatar;
    public GameServer server;
    [Header("Monitoring")]
    [ReadOnly]
    public List<Player> players = new List<Player>();
    private LogicTimer logicTimer;
    private int Sequence;

    private void Start()
    {
        logicTimer = new LogicTimer(OnLogicFrame);
        logicTimer.Start();
        localAvatar.id = -1;
        localAvatar.OnShoot.AddListener(ShootBullet);
    }

    private void Update()
    {
        logicTimer.Update();
        LerpPlayers(Time.deltaTime);
    }

    private void OnLogicFrame()
    {
        SendState();
    }
    public void OnPlayerConnected(int peerID)
    {
        Player newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
        newPlayer.SetNameOrientationTarget(localAvatar.headAlias);
        newPlayer.id = peerID;
        newPlayer.inputBuffer.Add(new PlayerInput()
        {
            Sequence = -1,
            HeadPosition = Vector3.zero,
            HeadRotation = Quaternion.identity,
            LeftHandPosition = Vector3.zero,
            LeftHandRotation = Quaternion.identity,
            RightHandPosition = Vector3.zero,
            RightHandRotation = Quaternion.identity,
        }
            );
        players.Add(newPlayer);
        InitMessage im = new InitMessage()
        {
            OwnId = peerID
        };
        server.SendInitMessage(im, peerID);
    }

    public void OnPlayerDisconnected(int peerID)
    {
        Player disconnectedPlayer = players.Find(x => x.id == peerID);
        players.Remove(disconnectedPlayer);
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
        Player p = players.Find(pl => pl.id == playerId);
        p.AddStateToBuffer(pi.Clone());
    }

    public PlayerState[] GetPlayersStates(int excludedPlayerId)
    {
        PlayerState[] playerStates = new PlayerState[players.Count];
        int playerStateCount = 0;
        for (int j = 0; j < players.Count; j++)
        {
            Player p = players[j];
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
                    LeftPointer = p.leftPointer,
                    RightPointer = p.rightPointer,
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

    private void SendState()
    {
        for (int i = 0; i < players.Count; i++)
        {
            PlayerState[] playerStates = GetPlayersStates(players[i].id);
            server.SendWorldState(
                new StateMessage()
                {
                    Sequence = Sequence,
                    Entities = new EntityState[0],
                    Players = playerStates
                },
                players[i].id
            );
        }
    }

    private void LerpPlayers(float delta)
    {
        for (int i = 0; i < players.Count; i++)
        {
            Player p = players[i];
            p.UpdatePosition(delta);
        }
    }

    public void ShootBullet(Gun gun)
    {
        GameObject bullet = Instantiate(bulletPrefab, gun.spawnPoint.position, gun.spawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 5, ForceMode.Impulse);
    }
}

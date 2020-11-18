using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerClient : MonoBehaviour
{
    public GameClient gameClient;
    public GameObject playerPrefab;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    private int avatarId;
    private readonly List<Player> players = new List<Player>();
    private float sendRate = 50 / 1000f;
    private float sendTimer = 0f;

    private void FixedUpdate()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendRate)
        {
            if (gameClient.Connected)
            {
                PlayerState avs = GetPlayerState();
                gameClient.SendPlayerState(avs);
            }
        }
    }

    /// <summary>
    /// Called when connected to the server
    /// </summary>
    public void OnConnected()
    {

    }
    /// <summary>
    /// Called when disconnected from the server
    /// </summary>
    public void OnDisconnected()
    {

    }
    /// <summary>
    /// Called when received initial data from the server
    /// </summary>
    public void OnInit(InitMessage im)
    {
        avatarId = im.OwnId;
    }
    /// <summary>
    /// Called when received fresh state data from the server
    /// </summary>
    public void OnState(StateMessage sm)
    {
        for (int i = 0; i < sm.Players.Length; i++)
        {
            Player player = players.Find(x => x.id == sm.Players[i].Id);
            if (player != null)
            {
                SetPlayerState(player, sm.Players[i]);
            }
            else if (sm.Players[i].Id != avatarId)
            {
                SpawnPlayer(sm.Players[i]);
            }
        }
        DespawnOldPlayers(sm.Players);
    }

    private void SpawnPlayer(PlayerState ps)
    {
        Player newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
        newPlayer.SetNameOrientationTarget(headGO);
        newPlayer.SetHeadPositionTarget(ps.HeadPosition);
        newPlayer.SetHeadRotationTarget(ps.HeadRotation);
        newPlayer.SetLeftHandPositionTarget(ps.LeftHandPosition);
        newPlayer.SetLeftHandRotationTarget(ps.LeftHandRotation);
        newPlayer.SetRightHandPositionTarget(ps.RightHandPosition);
        newPlayer.SetRightHandRotationTarget(ps.RightHandRotation);
        newPlayer.id = ps.Id;
        players.Add(newPlayer);
    }

    private void DespawnOldPlayers(PlayerState[] ps)
    {
        for (int i = 0; i < players.Count; i++)
        {
            Player oldPlayer = players[i];
            if (Array.Find(ps, x => x.Id == oldPlayer.id) == null)
            {
                players.Remove(oldPlayer);
                Destroy(oldPlayer.gameObject);
            }
        }
    }

    private void SetPlayerState(Player player, PlayerState ps)
    {
        player.headGO.transform.position = ps.HeadPosition;
        player.headGO.transform.rotation = ps.HeadRotation;
        player.leftGO.transform.position = ps.LeftHandPosition;
        player.leftGO.transform.rotation = ps.LeftHandRotation;
        player.rightGO.transform.position = ps.RightHandPosition;
        player.rightGO.transform.rotation = ps.RightHandRotation;
    }

    private PlayerState GetPlayerState()
    {
        return new PlayerState()
        {
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.rotation,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.rotation,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.rotation,
            Shooting = false
        };
    }

}

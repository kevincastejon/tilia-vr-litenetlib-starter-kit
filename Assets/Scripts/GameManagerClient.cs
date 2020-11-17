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
                AvatarState avs = GetAvatarState();
                gameClient.SendAvatarState(avs);
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
        newPlayer.headGO.transform.position = ps.HeadPosition;
        newPlayer.headGO.transform.eulerAngles = ps.HeadRotation;
        newPlayer.leftGO.transform.position = ps.LeftHandPosition;
        newPlayer.leftGO.transform.eulerAngles = ps.LeftHandRotation;
        newPlayer.rightGO.transform.position = ps.RightHandPosition;
        newPlayer.rightGO.transform.eulerAngles = ps.RightHandRotation;
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
        player.headGO.transform.eulerAngles = ps.HeadRotation;
        player.leftGO.transform.position = ps.LeftHandPosition;
        player.leftGO.transform.eulerAngles = ps.LeftHandRotation;
        player.rightGO.transform.position = ps.RightHandPosition;
        player.rightGO.transform.eulerAngles = ps.RightHandRotation;
    }

    private AvatarState GetAvatarState()
    {
        return new AvatarState()
        {
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.eulerAngles,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.eulerAngles,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.eulerAngles,
            Shooting = false
        };
    }

}

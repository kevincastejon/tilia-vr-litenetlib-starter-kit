using System.Collections.Generic;
using UnityEngine;

public class GameManagerServer : MonoBehaviour
{
    public GameServer server;
    public GameObject playerPrefab;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    private readonly List<Player> players = new List<Player>();
    private float sendRate = 50 / 1000f;
    private float sendTimer = 0f;

    private void Update()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendRate)
        {
            StateMessage sm = GetWorldState();
            server.SendWorldState(sm);
        }
    }

    public void OnClientConnected(int peerID)
    {
        Player newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
        newPlayer.SetNameOrientationTarget(headGO);
        newPlayer.GetComponent<Player>().id = peerID;
        players.Add(newPlayer);
        InitMessage im = new InitMessage()
        {
            OwnId = peerID
        };
        server.SendInitMessage(im, peerID);
    }

    public void OnClientDisconnected(int peerID)
    {
        Player disconnectedPlayer = players.Find(x => x.id == peerID);
        players.Remove(disconnectedPlayer);
        Destroy(disconnectedPlayer);
    }

    public void OnClientState(int peerID, PlayerState ps)
    {
        //Debug.Log("AvatarState : " + peerID);
        Player player = players.Find(x => x.GetComponent<Player>().id == peerID).GetComponent<Player>();
        player.SetHeadPositionTarget(ps.HeadPosition);
        player.SetHeadRotationTarget(ps.HeadRotation);
        player.SetLeftHandPositionTarget(ps.LeftHandPosition);
        player.SetLeftHandRotationTarget(ps.LeftHandRotation);
        player.SetRightHandPositionTarget(ps.RightHandPosition);
        player.SetRightHandRotationTarget(ps.RightHandRotation);
    }

    private StateMessage GetWorldState()
    {
        PlayerState[] playerStates = new PlayerState[players.Count+1];
        for (int i = 0; i < players.Count; i++)
        {
            Player p = players[i].GetComponent<Player>();
            playerStates[i] = new PlayerState()
            {
                Id = p.id,
                HeadPosition = p.headGO.transform.position,
                HeadRotation = p.headGO.transform.rotation,
                LeftHandPosition = p.leftGO.transform.position,
                LeftHandRotation = p.leftGO.transform.rotation,
                RightHandPosition = p.rightGO.transform.position,
                RightHandRotation = p.rightGO.transform.rotation,
                Shooting = false
            };
        }
        
        playerStates[players.Count] = new PlayerState()
        {
            Id = -1,
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.rotation,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.rotation,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.rotation,
            Shooting = false
        };

        StateMessage sm = new StateMessage()
        {
            Players = playerStates,
        };
        return sm;
    }
}

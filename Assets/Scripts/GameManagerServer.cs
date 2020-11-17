using System.Collections.Generic;
using UnityEngine;

public class GameManagerServer : MonoBehaviour
{
    public GameServer server;
    public GameObject playerPrefab;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    private readonly List<GameObject> players = new List<GameObject>();
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
        GameObject newPlayer = Instantiate(playerPrefab);
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
        GameObject disconnectedPlayer = players.Find(x => x.GetComponent<Player>().id == peerID);
        players.Remove(disconnectedPlayer);
        Destroy(disconnectedPlayer);
    }

    public void OnClientState(int peerID, AvatarState avatarState)
    {
        //Debug.Log("AvatarState : " + peerID);
        Player player = players.Find(x => x.GetComponent<Player>().id == peerID).GetComponent<Player>();
        player.headGO.transform.position = avatarState.HeadPosition;
        player.headGO.transform.eulerAngles = avatarState.HeadRotation;
        player.leftGO.transform.position = avatarState.LeftHandPosition;
        player.leftGO.transform.eulerAngles = avatarState.LeftHandRotation;
        player.rightGO.transform.position = avatarState.RightHandPosition;
        player.rightGO.transform.eulerAngles = avatarState.RightHandRotation;
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
                HeadRotation = p.headGO.transform.eulerAngles,
                LeftHandPosition = p.leftGO.transform.position,
                LeftHandRotation = p.leftGO.transform.eulerAngles,
                RightHandPosition = p.rightGO.transform.position,
                RightHandRotation = p.rightGO.transform.eulerAngles,
                Shooting = false
            };
        }
        
        playerStates[players.Count] = new PlayerState()
        {
            Id = -1,
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.eulerAngles,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.eulerAngles,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.eulerAngles,
            Shooting = false
        };

        StateMessage sm = new StateMessage()
        {
            Players = playerStates,
        };
        return sm;
    }
}

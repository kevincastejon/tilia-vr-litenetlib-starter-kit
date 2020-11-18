using System.Collections.Generic;
using UnityEngine;

public class GameManagerServer : MonoBehaviour
{
    public GameServer server;
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    public Gun gun;
    public bool shooting;
    private readonly List<Player> players = new List<Player>();
    private readonly List<Bullet> bullets = new List<Bullet>();
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

    public void SetShooting(bool shooting)
    {
        this.shooting = shooting;
        if (shooting)
        {
            Debug.Log("Spawn position: "+gun.spawnPoint.position);
            Debug.Log("Spawn rotation: "+gun.spawnPoint.rotation);
            Bullet bullet = Instantiate(bulletPrefab).GetComponent<Bullet>();
            bullet.transform.position = gun.spawnPoint.position;
            bullet.transform.rotation = gun.spawnPoint.rotation;
            bullet.GetComponent<Rigidbody>().AddRelativeForce(bullet.transform.forward * 1, ForceMode.Impulse);
            bullets.Add(bullet);
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
        player.shooting = ps.Shooting;
    }

    private StateMessage GetWorldState()
    {
        PlayerState[] playerStates = new PlayerState[players.Count + 1];
        for (int i = 0; i < players.Count; i++)
        {
            Player p = players[i];
            playerStates[i] = new PlayerState()
            {
                Id = p.id,
                HeadPosition = p.headGO.transform.position,
                HeadRotation = p.headGO.transform.rotation,
                LeftHandPosition = p.leftGO.transform.position,
                LeftHandRotation = p.leftGO.transform.rotation,
                RightHandPosition = p.rightGO.transform.position,
                RightHandRotation = p.rightGO.transform.rotation,
                Shooting = p.shooting
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
            Shooting = shooting
        };
        EntityState[] bulletStates = new EntityState[bullets.Count];
        for (int i = 0; i < bullets.Count; i++)
        {
            Bullet b = bullets[i];
            bulletStates[i] = new EntityState()
            {
                Id = b.id,
                Position = b.transform.position,
                Rotation = b.transform.rotation,
            };
        }
        StateMessage sm = new StateMessage()
        {
            Players = playerStates,
            Bullets = bulletStates
        };
        return sm;
    }
}

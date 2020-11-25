using System;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;

public class GameManagerClient : MonoBehaviour
{
    [ReadOnly]
    public int avatarId;
    [ReadOnly]
    public NetworkObject leftGrab;
    [ReadOnly]
    public NetworkObject rightGrab;
    [ReadOnly]
    public bool leftPointer;
    [ReadOnly]
    public bool rightPointer;
    [ReadOnly]
    public bool leftShooting;
    [ReadOnly]
    public bool rightShooting;
    public InteractorFacade leftInteractor;
    public InteractorFacade rightInteractor;
    public GameClient gameClient;
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    //public GameObject gunPrefab;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    private NetworkManager networkObjectManager;
    private readonly List<Player> players = new List<Player>();
    private float sendRate = 50 / 1000f;
    private float sendTimer = 0f;

    private void Start()
    {
        networkObjectManager = NetworkManager.GetInstance();
    }

    private void Update()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendRate)
        {
            sendTimer = 0f;
            if (gameClient.Connected)
            {
                PlayerInput pi = GetPlayerInput();
                gameClient.SendInput(pi);
            }
        }
    }

    public void SetShootingLeft(bool shooting)
    {
        leftShooting = shooting;
    }

    public void SetShootingRight(bool shooting)
    {
        rightShooting = shooting;
    }

    public void SetPointerLeft(bool activated)
    {
        leftPointer = activated;
    }

    public void SetPointerRight(bool activated)
    {
        rightPointer = activated;
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
        networkObjectManager.ClientSideSync(sm.Entities);
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
        player.SetHeadPositionTarget(ps.HeadPosition);
        player.SetHeadRotationTarget(ps.HeadRotation);
        player.SetLeftHandPositionTarget(ps.LeftHandPosition);
        player.SetLeftHandRotationTarget(ps.LeftHandRotation);
        player.SetRightHandPositionTarget(ps.RightHandPosition);
        player.SetRightHandRotationTarget(ps.RightHandRotation);
        player.SetLeftPointer(ps.LeftPointer);
        player.SetRightPointer(ps.RightPointer);
    }
    
    private PlayerInput GetPlayerInput()
    {
        return new PlayerInput()
        {
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.rotation.eulerAngles,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.rotation.eulerAngles,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.rotation.eulerAngles,
            LeftGrabId = leftGrab == null ? 0 : leftGrab.id,
            LeftGrabPosition = leftGrab == null ? Vector3.zero : leftGrab.transform.position,
            LeftGrabVelocity = leftGrab == null ? Vector3.zero : leftInteractor.VelocityTracker.GetVelocity(),
            LeftGrabRotation = leftGrab == null ? Vector3.zero : leftGrab.transform.rotation.eulerAngles,
            LeftGrabAngularVelocity = leftGrab == null ? Vector3.zero : leftInteractor.VelocityTracker.GetAngularVelocity(),
            RightGrabId = rightGrab == null ? 0 : rightGrab.id,
            RightGrabPosition = rightGrab == null ? Vector3.zero : rightGrab.transform.position,
            RightGrabVelocity = rightGrab == null ? Vector3.zero : rightInteractor.VelocityTracker.GetVelocity(),
            RightGrabRotation = rightGrab == null ? Vector3.zero : rightGrab.transform.rotation.eulerAngles,
            RightGrabAngularVelocity = rightGrab == null ? Vector3.zero : rightInteractor.VelocityTracker.GetAngularVelocity(),
            LeftShooting = leftShooting,
            RightShooting = rightShooting,
            LeftPointer = leftPointer,
            RightPointer = rightPointer,
        };
    }
}

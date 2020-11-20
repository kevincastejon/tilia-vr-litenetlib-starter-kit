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
    public NetworkGrabbableObject leftGrab;
    [ReadOnly]
    public NetworkGrabbableObject rightGrab;
    [ReadOnly]
    public bool leftShooting;
    [ReadOnly]
    public bool rightShooting;
    [ReadOnly]
    public bool leftPointer;
    [ReadOnly]
    public bool rightPointer;
    public GameClient gameClient;
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    public GameObject gunPrefab;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    public List<NetworkGrabbableObject> guns = new List<NetworkGrabbableObject>();
    private readonly List<Player> players = new List<Player>();
    private readonly List<NetworkObject> bullets = new List<NetworkObject>();
    private float sendRate = 50 / 1000f;
    private float sendTimer = 0f;

    private void Start()
    {
        guns.ForEach((NetworkGrabbableObject gun) =>
        {
            InteractableFacade interactable = gun.GetComponent<InteractableFacade>();
            interactable.Grabbed.AddListener((InteractorFacade interactor) => SetGrab(gun, interactor.name == "LeftInteractor"));
            interactable.Ungrabbed.AddListener((InteractorFacade interactor) => SetGrab(null, interactor.name == "LeftInteractor"));
        });
    }

    public void SetGrab(NetworkGrabbableObject obj, bool leftHand)
    {
        if (leftHand)
        {
            leftGrab = obj;
            if (obj != null)
            {
                obj.grabbed = true;
                obj.leftHand = true;
                obj.lastOwnerId = avatarId;
            }
            else
            {
                leftGrab.grabbed = false;
            }
        }
        else
        {
            rightGrab = obj;
            if (obj != null)
            {
                obj.grabbed = true;
                obj.leftHand = false;
                obj.lastOwnerId = avatarId;
            }
            else
            {
                rightGrab.grabbed = false;
            }
        }
    }

    private void Update()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendRate)
        {
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

        for (int i = 0; i < sm.Bullets.Length; i++)
        {
            NetworkObject bullet = bullets.Find(x => x.id == sm.Bullets[i].Id);
            if (bullet != null)
            {
                SetBulletState(bullet, sm.Bullets[i]);
            }
            else
            {
                SpawnBullet(sm.Bullets[i]);
            }
        }
        DespawnOldBullets(sm.Bullets);

        for (int i = 0; i < sm.Guns.Length; i++)
        {
            NetworkGrabbableObject gun = guns.Find(x => x.id == sm.Guns[i].Id);
            if (gun != null)
            {
                if (!gun.grabbed)
                {
                    SetGunState(gun, sm.Guns[i]);
                }
            }
            else
            {
                LinkLocalGun(sm.Guns[i]);
            }
        }
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
    private void SpawnBullet(EntityState bs)
    {
        NetworkObject newBullet = Instantiate(bulletPrefab).GetComponent<NetworkObject>();
        newBullet.SetPositionTarget(bs.Position);
        newBullet.SetRotationTarget(bs.Rotation);
        newBullet.id = bs.Id;
        bullets.Add(newBullet);
    }

    private void DespawnOldBullets(EntityState[] bs)
    {
        for (int i = 0; i < bullets.Count; i++)
        {
            NetworkObject oldBullet = bullets[i];
            if (Array.Find(bs, x => x.Id == oldBullet.id) == null)
            {
                bullets.Remove(oldBullet);
                Destroy(oldBullet.gameObject);
            }
        }
    }

    private void SetBulletState(NetworkObject bullet, EntityState bs)
    {
        bullet.transform.position = bs.Position;
        bullet.transform.rotation = bs.Rotation;
    }

    private void LinkLocalGun(EntityState gs)
    {
        NetworkGrabbableObject localGun = guns.Find((NetworkGrabbableObject g) => g.id == 0);
        localGun.SetPositionTarget(gs.Position);
        localGun.SetRotationTarget(gs.Rotation);
        localGun.id = gs.Id;
    }

    private void SetGunState(NetworkGrabbableObject gun, EntityState gs)
    {
        gun.transform.position = gs.Position;
        gun.transform.rotation = gs.Rotation;

    }

    private PlayerInput GetPlayerInput()
    {
        return new PlayerInput()
        {
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.rotation,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.rotation,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.rotation,
            LeftGrabId = leftGrab == null ? -1 : leftGrab.id,
            LeftGrabPosition = leftGrab == null ? Vector3.zero : leftGrab.transform.position,
            LeftGrabRotation = leftGrab == null ? Quaternion.identity : leftGrab.transform.rotation,
            RightGrabId = rightGrab == null ? -1 : rightGrab.id,
            RightGrabPosition = rightGrab == null ? Vector3.zero : rightGrab.transform.position,
            RightGrabRotation = rightGrab == null ? Quaternion.identity : rightGrab.transform.rotation,
            LeftShooting = leftShooting,
            RightShooting = rightShooting,
            LeftPointer = leftPointer,
            RightPointer = rightPointer,
        };
    }
}

using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using Unity.Collections;
using UnityEngine;

public class GameManagerServer : MonoBehaviour
{
    [ReadOnly]
    public bool shooting;
    public GameServer server;
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    [ReadOnly]
    public NetworkGrabbableObject leftGrab;
    [ReadOnly]
    public NetworkGrabbableObject rightGrab;
    public List<NetworkGrabbableObject> guns = new List<NetworkGrabbableObject>();
    private readonly List<Player> players = new List<Player>();
    private readonly List<NetworkObject> bullets = new List<NetworkObject>();
    private float sendRate = 50 / 1000f;
    private float sendTimer = 0f;
    private int serverId = -1;

    private void Start()
    {
        guns.ForEach((NetworkGrabbableObject gun) =>
        {
            InteractableFacade interactable = gun.GetComponent<InteractableFacade>();
            interactable.Grabbed.AddListener((InteractorFacade interactor) => SetGrab(gun, interactor.name == "LeftInteractor"));
            interactable.Ungrabbed.AddListener((InteractorFacade interactor) => SetGrab(null, interactor.name == "LeftInteractor"));
        });
    }

    private void Update()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendRate)
        {
            StateMessage sm = GetWorldState();
            server.SendWorldState(sm);
        }
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
                obj.lastOwnerId = serverId;
            }
            else
            {
                obj.grabbed = false;
            }
        }
        else
        {
            rightGrab = obj;
            if (obj != null)
            {
                obj.grabbed = true;
                obj.leftHand = false;
                obj.lastOwnerId = serverId;
            }
            else
            {
                obj.grabbed = false;
            }
        }
    }

    public void SetShootingLeft(bool shooting)
    {
        this.shooting = shooting;
        if (shooting)
        {
            Transform spawnPoint = leftGrab.GetComponent<Gun>().spawnPoint.transform;
            NetworkObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<NetworkObject>();
            bullet.GetComponent<Rigidbody>().AddRelativeForce(bullet.transform.forward * 1, ForceMode.Impulse);
            bullets.Add(bullet);
        }
    }

    public void SetShootingRight(bool shooting)
    {
        this.shooting = shooting;
        if (shooting)
        {
            Transform spawnPoint = rightGrab.GetComponent<Gun>().spawnPoint.transform;
            NetworkObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<NetworkObject>();
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 1, ForceMode.Impulse);
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

    public void OnClientInput(int peerID, PlayerInput pi)
    {
        //Debug.Log("AvatarState : " + peerID);
        Player player = players.Find(x => x.GetComponent<Player>().id == peerID).GetComponent<Player>();
        player.SetHeadPositionTarget(pi.HeadPosition);
        player.SetHeadRotationTarget(pi.HeadRotation);
        player.SetLeftHandPositionTarget(pi.LeftHandPosition);
        player.SetLeftHandRotationTarget(pi.LeftHandRotation);
        player.SetRightHandPositionTarget(pi.RightHandPosition);
        player.SetRightHandRotationTarget(pi.RightHandRotation);
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
            };
        }

        playerStates[players.Count] = new PlayerState()
        {
            Id = serverId,
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.rotation,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.rotation,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.rotation,
        };
        EntityState[] bulletStates = new EntityState[bullets.Count];
        for (int i = 0; i < bullets.Count; i++)
        {
            NetworkObject b = bullets[i];
            bulletStates[i] = new EntityState()
            {
                Id = b.id,
                Position = b.transform.position,
                Rotation = b.transform.rotation,
            };
        }
        EntityState[] gunStates = new EntityState[guns.Count];
        for (int i = 0; i < guns.Count; i++)
        {
            NetworkGrabbableObject g = guns[i];
            Vector3 position = g.transform.position;
            Quaternion rotation = g.transform.rotation;
            if (g.grabbed)
            {
                if (g.leftHand)
                {
                    position = g.lastOwnerId == -1 ? leftGrab.transform.position : players[g.lastOwnerId].leftGrabGO.transform.position;
                    rotation = g.lastOwnerId == -1 ? leftGrab.transform.rotation : players[g.lastOwnerId].leftGrabGO.transform.rotation;
                }
                else
                {
                    position = g.lastOwnerId == -1 ? rightGrab.transform.position : players[g.lastOwnerId].rightGrabGO.transform.position;
                    rotation = g.lastOwnerId == -1 ? rightGrab.transform.rotation : players[g.lastOwnerId].rightGrabGO.transform.rotation;
                }
            }
            gunStates[i] = new EntityState()
            {
                Id = g.id,
                Position = position,
                Rotation = rotation,
            };
        }
        StateMessage sm = new StateMessage()
        {
            Players = playerStates,
            Bullets = bulletStates,
            Guns = gunStates
        };
        return sm;
    }
}

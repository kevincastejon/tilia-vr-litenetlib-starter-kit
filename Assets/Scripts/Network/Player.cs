using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tilia.Indicators.ObjectPointers;

public class Player : MonoBehaviour
{

    public int id;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    public TextMeshProUGUI playerName;
    [ReadOnly]
    public bool leftPointerActivated;
    [ReadOnly]
    public bool rightPointerActivated;
    [ReadOnly]
    public int leftGrabId;
    [ReadOnly]
    public int rightGrabId;
    [ReadOnly]
    public bool leftShooting;
    [ReadOnly]
    public bool rightShooting;
    public PointerFacade leftPointer;
    public PointerFacade rightPointer;
    private GameObject nameOrientationTarget;
    private readonly List<PlayerState> stateBuffer = new List<PlayerState>();
    private PlayerState stateA;
    private PlayerState stateB;
    private float lerpMax = 1 / 60f;
    private float lerpTimer = 0f;
    public int stateBufferLength;

    private void FixedUpdate()
    {
        playerName.transform.rotation = Quaternion.LookRotation(playerName.transform.position - nameOrientationTarget.transform.position);

        bool isLerping = stateA != null && stateB != null;
        if (stateBuffer.Count >= 3 && !isLerping)
        {
            if (stateA == null)
            {
                stateA = stateBuffer[0];
                stateBuffer.RemoveAt(0);
            }
            stateB = stateBuffer[0];
            stateBuffer.RemoveAt(0);
            stateBufferLength = stateBuffer.Count;
            isLerping = true;
        }
        if (isLerping)
        {
            PlayerState esA = stateA;
            PlayerState esB = stateB;
            headGO.transform.position = Vector3.Lerp(esA.HeadPosition, esB.HeadPosition, lerpTimer / lerpMax);
            headGO.transform.rotation = Quaternion.Lerp(esA.HeadRotation, esB.HeadRotation, lerpTimer / lerpMax);
            leftGO.transform.position = Vector3.Lerp(esA.LeftHandPosition, esB.LeftHandPosition, lerpTimer / lerpMax);
            leftGO.transform.rotation = Quaternion.Lerp(esA.LeftHandRotation, esB.LeftHandRotation, lerpTimer / lerpMax);
            rightGO.transform.position = Vector3.Lerp(esA.RightHandPosition, esB.RightHandPosition, lerpTimer / lerpMax);
            rightGO.transform.rotation = Quaternion.Lerp(esA.RightHandRotation, esB.RightHandRotation, lerpTimer / lerpMax);
        }
        lerpTimer += Time.fixedDeltaTime;
        if (true)
        //if (lerpTimer >= lerpMax)
        {
            lerpTimer = 0f;
            stateA = stateB;
            stateB = null;
        }
    }

    public void AddStateToBuffer(PlayerState ps)
    {
        //if (DEVNetworkSwitcher.isServer)
        //{
        //Debug.Log("added state");
        //}
        stateBuffer.Add(ps);
        stateBufferLength = stateBuffer.Count;
    }

    public void ClearBuffer()
    {
        stateBuffer.Clear();
        stateA = null;
        stateB = null;
        lerpTimer = 0f;
    }

    public void SetLeftPointer(bool value)
    {
        leftPointerActivated = value;
        if (leftPointerActivated)
        {
            leftPointer.Activate();
        }
        else
        {
            leftPointer.Deactivate();
        }
    }

    public void SetRightPointer(bool value)
    {
        rightPointerActivated = value;
        if (rightPointerActivated)
        {
            rightPointer.Activate();
        }
        else
        {
            rightPointer.Deactivate();
        }
    }

    public void SetNameOrientationTarget(GameObject target)
    {
        nameOrientationTarget = target;
    }

    public void SetName(string name)
    {
        playerName.SetText(name);
    }
    public string GetName()
    {
        return playerName.text;
    }
}

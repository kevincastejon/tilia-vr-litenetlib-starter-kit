﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{

    public int id;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    public TextMeshProUGUI playerName;
    public bool leftPointerActivated;
    public bool rightPointerActivated;
    public bool shooting;
    private GameObject nameOrientationTarget;
    private LTDescr headMoveTween;
    private LTDescr headRotTween;
    private LTDescr leftHandMoveTween;
    private LTDescr leftHandRotTween;
    private LTDescr rightHandMoveTween;
    private LTDescr rightHandRotTween;
    private LTDescr leftGrabbedMoveTween;
    private LTDescr leftGrabbedRotTween;
    private LTDescr rightGrabbedMoveTween;
    private LTDescr rightGrabbedRotTween;
    private float lastHeadPosUpdate;
    private float lastHeadRotUpdate;
    private float lastLeftHandPosUpdate;
    private float lastLeftHandRotUpdate;
    private float lastRightHandPosUpdate;
    private float lastRightHandRotUpdate;
    private float lastLeftGrabbedPosUpdate;
    private float lastLeftGrabbedRotUpdate;
    private float lastRightGrabbedPosUpdate;
    private float lastRightGrabbedRotUpdate;

    private void Update()
    {
        playerName.transform.rotation = Quaternion.LookRotation(playerName.transform.position - nameOrientationTarget.transform.position);
        lastHeadPosUpdate += Time.deltaTime;
        lastHeadRotUpdate += Time.deltaTime;
        lastLeftHandPosUpdate += Time.deltaTime;
        lastLeftHandRotUpdate += Time.deltaTime;
        lastRightHandPosUpdate += Time.deltaTime;
        lastRightHandRotUpdate += Time.deltaTime;
    }

    public void SetNameOrientationTarget(GameObject target)
    {
        nameOrientationTarget = target;
    }

    public void SetHeadPositionTarget(Vector3 posTarget)
    {
        if (headMoveTween != null)
        {
            LeanTween.cancel(headMoveTween.id);
        }
        headMoveTween = LeanTween.move(headGO, posTarget, lastHeadPosUpdate);
        headMoveTween.setOnComplete(() => headMoveTween = null);
        lastHeadPosUpdate = 0;
    }
    public void SetHeadRotationTarget(Quaternion rotTarget)
    {
        if (headRotTween != null)
        {
            LeanTween.cancel(headRotTween.id);
        }
        headRotTween = LeanTween.rotate(headGO, rotTarget.eulerAngles, lastHeadRotUpdate);
        headRotTween.setOnComplete(() => headRotTween = null);
        lastHeadRotUpdate = 0;
    }
    public void SetLeftHandPositionTarget(Vector3 posTarget)
    {
        if (leftHandMoveTween != null)
        {
            LeanTween.cancel(leftHandMoveTween.id);
        }
        leftHandMoveTween = LeanTween.move(leftGO, posTarget, lastLeftHandPosUpdate);
        leftHandMoveTween.setOnComplete(() => leftHandMoveTween = null);
        lastLeftHandPosUpdate = 0;
    }
    public void SetLeftHandRotationTarget(Quaternion rotTarget)
    {
        if (leftHandRotTween != null)
        {
            LeanTween.cancel(leftHandRotTween.id);
        }
        leftHandRotTween = LeanTween.rotate(leftGO, rotTarget.eulerAngles, lastLeftHandRotUpdate);
        leftHandRotTween.setOnComplete(() => leftHandRotTween = null);
        lastLeftHandRotUpdate = 0;
    }
    public void SetRightHandPositionTarget(Vector3 posTarget)
    {
        if (rightHandMoveTween != null)
        {
            LeanTween.cancel(rightHandMoveTween.id);
        }
        rightHandMoveTween = LeanTween.move(rightGO, posTarget, lastRightHandPosUpdate);
        rightHandMoveTween.setOnComplete(() => rightHandMoveTween = null);
        lastRightHandPosUpdate = 0;
    }
    public void SetRightHandRotationTarget(Quaternion rotTarget)
    {
        if (rightHandRotTween != null)
        {
            LeanTween.cancel(rightHandRotTween.id);
        }
        rightHandRotTween = LeanTween.rotate(rightGO, rotTarget.eulerAngles, lastRightHandRotUpdate);
        rightHandRotTween.setOnComplete(() => rightHandRotTween = null);
        lastRightHandRotUpdate = 0;
    }
    public void SetLeftGrabbedPositionTarget(Vector3 posTarget)
    {
        if (leftGrabbedMoveTween != null)
        {
            LeanTween.cancel(leftGrabbedMoveTween.id);
        }
        leftGrabbedMoveTween = LeanTween.move(leftGO, posTarget, lastLeftGrabbedPosUpdate);
        leftGrabbedMoveTween.setOnComplete(() => leftGrabbedMoveTween = null);
        lastLeftGrabbedPosUpdate = 0;
    }
    public void SetLeftGrabbedRotationTarget(Quaternion rotTarget)
    {
        if (leftGrabbedRotTween != null)
        {
            LeanTween.cancel(leftGrabbedRotTween.id);
        }
        leftGrabbedRotTween = LeanTween.rotate(leftGO, rotTarget.eulerAngles, lastLeftGrabbedRotUpdate);
        leftGrabbedRotTween.setOnComplete(() => leftGrabbedRotTween = null);
        lastLeftGrabbedRotUpdate = 0;
    }
    public void SetRightGrabbedPositionTarget(Vector3 posTarget)
    {
        if (rightGrabbedMoveTween != null)
        {
            LeanTween.cancel(rightGrabbedMoveTween.id);
        }
        rightGrabbedMoveTween = LeanTween.move(rightGO, posTarget, lastRightGrabbedPosUpdate);
        rightGrabbedMoveTween.setOnComplete(() => rightGrabbedMoveTween = null);
        lastRightGrabbedPosUpdate = 0;
    }
    public void SetRightGrabbedRotationTarget(Quaternion rotTarget)
    {
        if (rightGrabbedRotTween != null)
        {
            LeanTween.cancel(rightGrabbedRotTween.id);
        }
        rightGrabbedRotTween = LeanTween.rotate(rightGO, rotTarget.eulerAngles, lastRightGrabbedRotUpdate);
        rightGrabbedRotTween.setOnComplete(() => rightGrabbedRotTween = null);
        lastRightGrabbedRotUpdate = 0;
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

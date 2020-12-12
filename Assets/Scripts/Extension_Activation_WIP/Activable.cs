using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ActivationEvent : UnityEvent { };
public class Activable : MonoBehaviour
{
    public bool disabled;
    public ActivationEvent OnActivated = new ActivationEvent();
    //public ActivationEvent OnDeactivated = new ActivationEvent();
    public void Activate()
    {
        if (disabled)
        {
            return;
        }
        OnActivated.Invoke();
    }
    //public void Deactivate()
    //{
    //    OnDeactivated.Invoke();
    //}
}

using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;

[RequireComponent(typeof(InteractableFacade))]
public class DisableCollisionsOnGrab : MonoBehaviour
{
    [Header("Reference settings")]
    public List<Collider> colliders = new List<Collider>();
    [Header("Toggle")]
    public bool collidesOnGrab = false;
    private InteractableFacade interactableFacade;
    // Start is called before the first frame update
    void Awake()
    {
        interactableFacade = GetComponent<InteractableFacade>();
        interactableFacade.Grabbed.AddListener(OnGrab);
        interactableFacade.Ungrabbed.AddListener(OnUngrab);
    }

    private void OnGrab(InteractorFacade interactor)
    {
        if (!collidesOnGrab)
        {
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
        }
    }

    private void OnUngrab(InteractorFacade interactor)
    {
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }
}

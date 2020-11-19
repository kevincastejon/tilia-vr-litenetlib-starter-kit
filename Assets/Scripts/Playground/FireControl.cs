using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;

public class FireControl : MonoBehaviour
{
    public GameObject leftFireControl;
    public GameObject rightFireControl;
    public void Grab(InteractorFacade interactor)
    {
        if (interactor.name == "LeftInteractor")
        {
            leftFireControl.SetActive(true);
        }
        else
        {
            rightFireControl.SetActive(true);
        }
    }
    public void Ungrab(InteractorFacade interactor)
    {
        if (interactor.name == "LeftInteractor")
        {
            leftFireControl.SetActive(false);
        }
        else
        {
            rightFireControl.SetActive(false);
        }
    }
}

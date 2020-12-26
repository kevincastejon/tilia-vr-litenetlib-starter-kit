using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Activable))]
public class DisableActivableClientSide : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (!NetworkManager.isServer)
        {
            GetComponent<Activable>().disabled = true;
        }
    }
}

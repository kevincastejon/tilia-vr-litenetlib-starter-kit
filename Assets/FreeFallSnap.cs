using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.SnapZone;
using UnityEngine;

[RequireComponent(typeof(SnapZoneFacade))]
public class FreeFallSnap : MonoBehaviour
{
    SnapZoneFacade snapZone;
    private void Start()
    {
        snapZone = GetComponent<SnapZoneFacade>();
    }
    public void OnCollision(GameObject obj)
    {
        NetworkGrabbableObject gobj = obj.GetComponent<NetworkGrabbableObject>();
        if (!gobj.grabbed)
        {
            snapZone.Snap(obj);
        }
    }
    public void OnSnap(GameObject obj)
    {
        NetworkGrabbableObject gobj = obj.GetComponent<NetworkGrabbableObject>();
        gobj.snapContainer = snapZone;
    }
}

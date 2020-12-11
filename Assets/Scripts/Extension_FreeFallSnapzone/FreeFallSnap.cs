using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.SnapZone;
using UnityEngine;

[RequireComponent(typeof(SnapZoneFacade))]
public class FreeFallSnap : MonoBehaviour
{
    private SnapZoneFacade snapZone;
    private void Awake()
    {
        snapZone = GetComponent<SnapZoneFacade>();
    }
    public void OnCollision(GameObject obj)
    {
        InteractableFacade gobj = obj.GetComponent<InteractableFacade>();
        if (!gobj.IsGrabbed)
        {
            snapZone.Snap(obj);
        }
    }
}

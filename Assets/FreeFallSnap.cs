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
        if (!obj.GetComponent<InteractableFacade>().IsGrabbed)
        {
            snapZone.Snap(obj);
        }
    }
}

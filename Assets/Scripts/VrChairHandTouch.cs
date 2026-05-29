using UnityEngine;
using Unity.Netcode;

public class VRChairHandTouch : NetworkBehaviour
{
    public VRChairController chair;
    public bool isLeftHand;

    private bool IsNetworkActive()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    }

    private bool IsLocalMode => !IsNetworkActive();

    public override void OnNetworkSpawn()
    {
        if (!chair) return;

        if (!IsLocalMode && !chair.IsOwner)
        {
            enabled = false;
        }
    }

    private bool IsAllowed()
    {
        if (IsLocalMode) return true;
        return chair != null && chair.IsOwner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsAllowed()) return;
        if (!other.CompareTag(chair.touchTag)) return;

        if (isLeftHand)
            chair.SetLeftTouching(true);
        else
            chair.SetRightTouching(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsAllowed()) return;
        if (!other.CompareTag(chair.touchTag)) return;

        if (isLeftHand)
            chair.SetLeftTouching(false);
        else
            chair.SetRightTouching(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsAllowed()) return;
        if (!other.CompareTag(chair.touchTag)) return;

        Vector3 normal =
            (transform.position - other.ClosestPoint(transform.position)).normalized;

        if (isLeftHand)
            chair.SetLeftNormal(normal);
        else
            chair.SetRightNormal(normal);
    }
}
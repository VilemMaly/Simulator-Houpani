using UnityEngine;

public class VRChairHandTouch : MonoBehaviour
{
    public VRChairController chair;
    public bool isLeftHand;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(chair.touchTag))
            return;

        if (isLeftHand)
            chair.SetLeftTouching(true);
        else
            chair.SetRightTouching(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(chair.touchTag))
            return;

        if (isLeftHand)
            chair.SetLeftTouching(false);
        else
            chair.SetRightTouching(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(chair.touchTag))
            return;

        Vector3 normal = (transform.position - other.ClosestPoint(transform.position)).normalized;

        if (isLeftHand)
            chair.SetLeftNormal(normal);
        else
            chair.SetRightNormal(normal);
    }
}
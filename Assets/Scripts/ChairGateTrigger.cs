using UnityEngine;

public class ChairGateTrigger : MonoBehaviour
{
    public enum GateType
    {
        First,
        Second
    }

    public GateType gateType;

    public ChairPassCounter counter;

    private void OnTriggerEnter(Collider other)
    {
        if (gateType == GateType.First)
        {
            counter.HitFirst(other);
        }
        else
        {
            counter.HitSecond(other);
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyImpulseController : MonoBehaviour
{
    [Header("Rotation (Torque)")]
    public bool useTorque = true;
    public Vector3 torqueDirection = Vector3.up;
    public float torqueStrength = 100f;
    public bool randomTorqueDirection = false;

    [Header("Movement (Force)")]
    public bool useForce = true;
    public Vector3 forceDirection = Vector3.up;
    public float forceStrength = 50f;
    public bool randomForceDirection = false;

    [Header("Space")]
    public bool useLocalSpace = true;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        ApplyImpulse();
    }

    public void ApplyImpulse()
    {
        if (useTorque)
        {
            Vector3 dir = randomTorqueDirection ? Random.onUnitSphere : torqueDirection.normalized;

            if (useLocalSpace)
                dir = transform.TransformDirection(dir);

            rb.AddTorque(dir * torqueStrength, ForceMode.Impulse);
        }

        if (useForce)
        {
            Vector3 dir = randomForceDirection ? Random.onUnitSphere : forceDirection.normalized;

            if (useLocalSpace)
                dir = transform.TransformDirection(dir);

            rb.AddForce(dir * forceStrength, ForceMode.Impulse);
        }
    }

    // pro testování v editoru
    [ContextMenu("Apply Impulse Again")]
    void DebugApply()
    {
        ApplyImpulse();
    }
}
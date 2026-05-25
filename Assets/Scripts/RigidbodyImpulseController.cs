using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyImpulseController : MonoBehaviour
{
    [System.Serializable]
    public class ForceAxis
    {
        public bool enabled = true;
        public Vector3 direction = Vector3.up;
        public float strength = 50f;
        public bool randomDirection = false;
    }

    [Header("ROTATION (Torque) - 3 axes")]
    public ForceAxis torqueA = new ForceAxis() { direction = Vector3.up };
    public ForceAxis torqueB = new ForceAxis() { direction = Vector3.right };
    public ForceAxis torqueC = new ForceAxis() { direction = Vector3.forward };

    [Header("MOVEMENT (Force) - 3 axes")]
    public ForceAxis forceA = new ForceAxis() { direction = Vector3.up };
    public ForceAxis forceB = new ForceAxis() { direction = Vector3.right };
    public ForceAxis forceC = new ForceAxis() { direction = Vector3.forward };

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
        ApplyTorque(torqueA);
        ApplyTorque(torqueB);
        ApplyTorque(torqueC);

        ApplyForce(forceA);
        ApplyForce(forceB);
        ApplyForce(forceC);
    }

    void ApplyTorque(ForceAxis axis)
    {
        if (!axis.enabled) return;

        Vector3 dir = axis.randomDirection ? Random.onUnitSphere : axis.direction.normalized;

        if (useLocalSpace)
            dir = transform.TransformDirection(dir);

        rb.AddTorque(dir * axis.strength, ForceMode.Impulse);
    }

    void ApplyForce(ForceAxis axis)
    {
        if (!axis.enabled) return;

        Vector3 dir = axis.randomDirection ? Random.onUnitSphere : axis.direction.normalized;

        if (useLocalSpace)
            dir = transform.TransformDirection(dir);

        rb.AddForce(dir * axis.strength, ForceMode.Impulse);
    }

    [ContextMenu("Apply Impulse Again")]
    void DebugApply()
    {
        ApplyImpulse();
    }
}
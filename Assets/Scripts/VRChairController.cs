using UnityEngine;
using UnityEngine.InputSystem;

public class VRChairController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;

    [Header("Center Of Mass")]
    public Transform centerOfMass;

    [Header("Player")]
    public Transform playerTransform;

    [Header("Hands")]
    public SphereCollider leftHandCollider;
    public SphereCollider rightHandCollider;

    public string touchTag = "touch";

    [Header("Hand Movement")]
    public float handForceMultiplier = 8f;
    public float maxHandForce = 20f;

    [Header("Thrusters")]
    public Transform leftThruster;
    public Transform rightThruster;

    public float thrusterForce = 150f;

    [Header("Input Actions Asset")]
    public InputActionReference leftThrusterAction;
    public InputActionReference rightThrusterAction;

    [Header("Limits")]
    public float maxVelocity = 20f;

    [Header("Debug")]
    public bool debugLogs = true;

    private Vector3 previousLeftLocalPos;
    private Vector3 previousRightLocalPos;

    private bool leftTouching;
    private bool rightTouching;

    private Vector3 leftNormal;
    private Vector3 rightNormal;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        leftThrusterAction?.action.Enable();
        rightThrusterAction?.action.Enable();
    }

    private void OnDisable()
    {
        leftThrusterAction?.action.Disable();
        rightThrusterAction?.action.Disable();
    }

    private void Start()
    {
        rb.maxAngularVelocity = 50f;

        if (centerOfMass != null)
        {
            rb.centerOfMass =
                transform.InverseTransformPoint(centerOfMass.position);
        }

        previousLeftLocalPos =
            transform.InverseTransformPoint(leftHandCollider.transform.position);

        previousRightLocalPos =
            transform.InverseTransformPoint(rightHandCollider.transform.position);
    }

    private void FixedUpdate()
    {
        HandleHandMovement();
        HandleThrusters();
        LimitVelocity();
    }

    private void HandleHandMovement()
    {
        Vector3 currentLeftLocalPos =
            transform.InverseTransformPoint(leftHandCollider.transform.position);

        Vector3 currentRightLocalPos =
            transform.InverseTransformPoint(rightHandCollider.transform.position);

        Vector3 leftLocalVelocity =
            (currentLeftLocalPos - previousLeftLocalPos) / Time.fixedDeltaTime;

        Vector3 rightLocalVelocity =
            (currentRightLocalPos - previousRightLocalPos) / Time.fixedDeltaTime;

        previousLeftLocalPos = currentLeftLocalPos;
        previousRightLocalPos = currentRightLocalPos;

        Vector3 leftWorldVelocity =
            transform.TransformDirection(leftLocalVelocity);

        Vector3 rightWorldVelocity =
            transform.TransformDirection(rightLocalVelocity);

        ApplyHand(leftTouching, leftWorldVelocity, leftNormal);
        ApplyHand(rightTouching, rightWorldVelocity, rightNormal);
    }

    private void ApplyHand(bool touching, Vector3 velocity, Vector3 normal)
    {
        if (!touching) return;

        if (playerTransform == null) return;

        Vector3 toPlayer =
            (playerTransform.position - transform.position).normalized;

        float dot = Vector3.Dot(normal, toPlayer);

        if (dot <= 0f)
            return;

        Vector3 projected =
            Vector3.Project(velocity, normal);

        Vector3 force =
            -projected * handForceMultiplier;

        force = Vector3.ClampMagnitude(force, maxHandForce);

        rb.AddForce(force, ForceMode.Acceleration);
    }

    private void HandleThrusters()
    {
        if (leftThrusterAction?.action.IsPressed() == true)
            ApplyThruster(leftThruster, "LEFT");

        if (rightThrusterAction?.action.IsPressed() == true)
            ApplyThruster(rightThruster, "RIGHT");
    }

    private void ApplyThruster(Transform thruster, string name)
    {
        if (thruster == null) return;

        Vector3 force = thruster.forward * thrusterForce;

        rb.AddForceAtPosition(force, thruster.position, ForceMode.Force);
    }

    private void LimitVelocity()
    {
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
    }

    public void SetLeftTouching(bool value)
    {
        leftTouching = value;
    }

    public void SetRightTouching(bool value)
    {
        rightTouching = value;
    }

    // NOVÉ: nastavování normály z trigger skriptu
    public void SetLeftNormal(Vector3 normal)
    {
        leftNormal = normal.normalized;
    }

    public void SetRightNormal(Vector3 normal)
    {
        rightNormal = normal.normalized;
    }
}
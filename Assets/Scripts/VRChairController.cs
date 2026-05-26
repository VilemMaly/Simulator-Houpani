using UnityEngine;
using UnityEngine.InputSystem;

public class VRChairController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;

    [Space]

    public SphereCollider leftHandCollider;
    public SphereCollider rightHandCollider;

    [Space]

    public Transform centerOfMass;

    [Space]

    public Transform leftThruster;
    public Transform rightThruster;

    [Header("Hand Push Movement")]
    public float handVelocityMultiplier = 1.5f;
    public string touchTag = "touch";

    [Header("Thrusters")]
    public InputActionProperty leftThrusterAction;
    public InputActionProperty rightThrusterAction;

    public float thrusterForce = 40f;

    [Header("Physics")]
    public float maxVelocity = 15f;
    public float angularDragWhileGrounded = 2f;
    public float dragWhileGrounded = 0.2f;

    private Vector3 previousLeftHandPos;
    private Vector3 previousRightHandPos;

    private Vector3 leftHandVelocity;
    private Vector3 rightHandVelocity;

    private bool leftTouching;
    private bool rightTouching;

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (centerOfMass != null)
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);

        previousLeftHandPos = leftHandCollider.transform.position;
        previousRightHandPos = rightHandCollider.transform.position;

        rb.maxAngularVelocity = 50f;
    }

    private void FixedUpdate()
    {
        UpdateHandVelocities();

        ApplyHandMovement();

        ApplyThrusters();

        LimitVelocity();

        rb.linearDamping = dragWhileGrounded;
        rb.angularDamping = angularDragWhileGrounded;
    }

    private void UpdateHandVelocities()
    {
        leftHandVelocity =
            (leftHandCollider.transform.position - previousLeftHandPos)
            / Time.fixedDeltaTime;

        rightHandVelocity =
            (rightHandCollider.transform.position - previousRightHandPos)
            / Time.fixedDeltaTime;

        previousLeftHandPos = leftHandCollider.transform.position;
        previousRightHandPos = rightHandCollider.transform.position;
    }

    private void ApplyHandMovement()
    {
        if (leftTouching)
        {
            Vector3 force =
                leftHandVelocity * handVelocityMultiplier;

            rb.AddForce(force, ForceMode.Acceleration);
        }

        if (rightTouching)
        {
            Vector3 force =
                rightHandVelocity * handVelocityMultiplier;

            rb.AddForce(force, ForceMode.Acceleration);
        }
    }

    private void ApplyThrusters()
    {
        bool leftActive =
            leftThrusterAction.action != null &&
            leftThrusterAction.action.IsPressed();

        bool rightActive =
            rightThrusterAction.action != null &&
            rightThrusterAction.action.IsPressed();

        if (leftActive)
        {
            ApplyThrusterForce(leftThruster);
        }

        if (rightActive)
        {
            ApplyThrusterForce(rightThruster);
        }
    }

    private void ApplyThrusterForce(Transform thruster)
    {
        if (thruster == null)
            return;

        rb.AddForceAtPosition(
            thruster.forward * thrusterForce,
            thruster.position,
            ForceMode.Force
        );
    }

    private void LimitVelocity()
    {
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * maxVelocity;
        }
    }

    public void SetLeftTouching(bool touching)
    {
        leftTouching = touching;
    }

    public void SetRightTouching(bool touching)
    {
        rightTouching = touching;
    }
}
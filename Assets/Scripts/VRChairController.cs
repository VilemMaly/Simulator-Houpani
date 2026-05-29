using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class VRChairController : NetworkBehaviour
{
    [Header("References")]
    public Rigidbody rb;

    [Header("Camera (LOCAL ONLY)")]
    public GameObject playerCameraRoot;

    [Header("Disable For Non Owner")]
    public GameObject nonOwnerColliderParent;

    [Header("Center Of Mass")]
    public Transform centerOfMass;

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

    private bool IsNetworkActive()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    }

    private bool IsLocalMode => !IsNetworkActive();

    private bool IsLocalPlayer()
    {
        // offline → vždy true
        if (IsLocalMode) return true;

        // online → jen owner
        return IsOwner;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsLocalPlayer())
        {
            if (playerCameraRoot != null)
                playerCameraRoot.SetActive(false);

            // odstraní collider parent jen pro non-owner instance
            if (nonOwnerColliderParent != null)
                Destroy(nonOwnerColliderParent);

            enabled = false;
            return;
        }

        rb.maxAngularVelocity = 50f;

        if (centerOfMass != null)
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);

        previousLeftLocalPos =
            transform.InverseTransformPoint(leftHandCollider.transform.position);

        previousRightLocalPos =
            transform.InverseTransformPoint(rightHandCollider.transform.position);
    }

    private void OnEnable()
    {
        if (!IsLocalPlayer()) return;

        leftThrusterAction?.action.Enable();
        rightThrusterAction?.action.Enable();
    }

    private void OnDisable()
    {
        if (!IsLocalPlayer()) return;

        leftThrusterAction?.action.Disable();
        rightThrusterAction?.action.Disable();
    }

    private void FixedUpdate()
    {
        if (!IsLocalPlayer()) return;
        if (IsNetworkActive() && !IsSpawned) return;

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

        Vector3 leftWorldVelocity = transform.TransformDirection(leftLocalVelocity);
        Vector3 rightWorldVelocity = transform.TransformDirection(rightLocalVelocity);

        ApplyHand(leftTouching, leftWorldVelocity, leftNormal);
        ApplyHand(rightTouching, rightWorldVelocity, rightNormal);
    }

    private void ApplyHand(bool touching, Vector3 velocity, Vector3 normal)
    {
        if (!touching) return;
        if (playerCameraRoot == null) return;

        Vector3 toPlayer =
            (playerCameraRoot.transform.position - transform.position).normalized;

        float dot = Vector3.Dot(normal, toPlayer);
        if (dot <= 0f) return;

        Vector3 projected = Vector3.Project(velocity, normal);

        Vector3 force = -projected * handForceMultiplier;
        force = Vector3.ClampMagnitude(force, maxHandForce);

        rb.AddForce(force, ForceMode.Acceleration);
    }

    private void HandleThrusters()
    {
        if (leftThrusterAction?.action.IsPressed() == true)
            ApplyThruster(leftThruster);

        if (rightThrusterAction?.action.IsPressed() == true)
            ApplyThruster(rightThruster);
    }

    private void ApplyThruster(Transform thruster)
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

    public void SetLeftTouching(bool value) => leftTouching = value;
    public void SetRightTouching(bool value) => rightTouching = value;

    public void SetLeftNormal(Vector3 normal) => leftNormal = normal.normalized;
    public void SetRightNormal(Vector3 normal) => rightNormal = normal.normalized;
}
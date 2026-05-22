using UnityEngine;
using UnityEngine.InputSystem;

public class ChairSystem : MonoBehaviour
{
    [Header("Fyzikální nastavení")]
    public Rigidbody chairRigidbody;
    public float torqueStrength = 20f;
    public float fallThreshold = 45f;
    public Transform centerOfMassTarget;

    [Header("Vstupy (Oculus Quest 2)")]
    public InputActionProperty forwardAction;
    public InputActionProperty backwardAction;

    [Header("Swing systém")]
    public float swingAngleA = 10f;
    public float swingAngleB = 25f;
    public int swingPoints = 10;

    [Header("Score display")]
    public Cisla cislaDisplay;

    [Header("DEBUG")]
    public bool debugAngles = true;
    public float debugInterval = 1f;

    private float debugTimer;

    private int score = 0;
    private bool isFalling = false;
    private bool wasInSwingZone = false;

    private Vector3 startPos;
    private Quaternion startRot;

    private void OnEnable()
    {
        if (forwardAction.action != null) forwardAction.action.Enable();
        if (backwardAction.action != null) backwardAction.action.Enable();
    }

    private void OnDisable()
    {
        if (forwardAction.action != null) forwardAction.action.Disable();
        if (backwardAction.action != null) backwardAction.action.Disable();
    }

    void Start()
    {
        Debug.Log("[ChairSystem] START");

        if (chairRigidbody == null)
            chairRigidbody = GetComponentInChildren<Rigidbody>();

        startPos = chairRigidbody.transform.position;
        startRot = chairRigidbody.transform.rotation;

        if (centerOfMassTarget != null)
        {
            chairRigidbody.centerOfMass =
                chairRigidbody.transform.InverseTransformPoint(centerOfMassTarget.position);
        }

        if (cislaDisplay != null)
            cislaDisplay.Write(score);
    }

    void FixedUpdate()
    {
        if (chairRigidbody == null) return;

        float forward = forwardAction.action?.ReadValue<float>() ?? 0f;
        float backward = backwardAction.action?.ReadValue<float>() ?? 0f;

        float moveInput = forward - backward;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            chairRigidbody.AddRelativeTorque(
                Vector3.right * moveInput * torqueStrength,
                ForceMode.Acceleration
            );
        }

        CheckSwing();
        CheckFall();
        DebugAngles();
    }

    private void CheckSwing()
    {
        if (isFalling) return;

        float angle = Vector3.Angle(chairRigidbody.transform.up, Vector3.up);

        if (angle > swingAngleA && angle < swingAngleB)
            wasInSwingZone = true;

        if (wasInSwingZone && angle < swingAngleA)
        {
            wasInSwingZone = false;
            RegisterSwing();
        }
    }

    private void RegisterSwing()
    {
        score += swingPoints;

        Debug.Log($"[SWING] +{swingPoints} SCORE = {score}");

        if (cislaDisplay != null)
            cislaDisplay.Write(score);
    }

    private void CheckFall()
    {
        float angle = Vector3.Angle(chairRigidbody.transform.up, Vector3.up);

        if (angle > fallThreshold && !isFalling)
        {
            Debug.LogError("[FALL] Židle spadla → RESET");

            isFalling = true;
            ResetGame();
        }
    }

    private void ResetGame()
    {
        score = 0;
        wasInSwingZone = false;
        isFalling = false;

        chairRigidbody.linearVelocity = Vector3.zero;
        chairRigidbody.angularVelocity = Vector3.zero;

        chairRigidbody.transform.position = startPos;
        chairRigidbody.transform.rotation = startRot;

        if (cislaDisplay != null)
            cislaDisplay.Write(score);

        Debug.Log("[RESET] Game reset hotový");
    }

    private void DebugAngles()
    {
        if (!debugAngles) return;

        debugTimer += Time.fixedDeltaTime;

        if (debugTimer >= debugInterval)
        {
            debugTimer = 0f;

            float angle = Vector3.Angle(chairRigidbody.transform.up, Vector3.up);

            Debug.Log($"[DEBUG] Angle: {angle:F2} | Vel: {chairRigidbody.angularVelocity}");
        }
    }
}
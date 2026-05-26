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

    [Tooltip("Jak moc roste multiplier nad swingAngleB")]
    public float extraMultiplierScale = 0.01f;

    [Header("Score display")]
    public Cisla cislaDisplay;

    [Header("Popup")]
    public Popup scorePopup;

    [Header("DEBUG")]
    public bool debugAngles = true;
    public float debugInterval = 1f;

    private float debugTimer;

    private int score = 0;
    private bool isFalling = false;

    // Swing state
    private bool swingStarted = false;

    // maximum positive angle reached during swing
    private float maxPositiveAngle = 0f;

    // prevents spam around center
    private bool alreadyScoredThisSwing = false;

    private Vector3 startPos;
    private Quaternion startRot;

    private void OnEnable()
    {
        if (forwardAction.action != null)
            forwardAction.action.Enable();

        if (backwardAction.action != null)
            backwardAction.action.Enable();
    }

    private void OnDisable()
    {
        if (forwardAction.action != null)
            forwardAction.action.Disable();

        if (backwardAction.action != null)
            backwardAction.action.Disable();
    }

    private void Start()
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

    private void FixedUpdate()
    {
        if (chairRigidbody == null)
            return;

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

    private float GetSignedAngle()
    {
        return Vector3.SignedAngle(
            Vector3.up,
            chairRigidbody.transform.up,
            chairRigidbody.transform.right
        );
    }

    private void CheckSwing()
    {
        if (isFalling)
            return;

        float angle = GetSignedAngle();

        // =========================================
        // START SWING
        // =========================================
        if (!swingStarted)
        {
            // start only when entering positive side
            if (angle > swingAngleA)
            {
                swingStarted = true;

                maxPositiveAngle = angle;

                alreadyScoredThisSwing = false;
            }

            return;
        }

        // =========================================
        // TRACK ONLY POSITIVE MAXIMUM
        // =========================================
        if (angle > maxPositiveAngle)
        {
            maxPositiveAngle = angle;
        }

        // =========================================
        // SWING FINISHED
        // =========================================
        // once chair returns back below A
        if (angle < swingAngleA)
        {
            if (!alreadyScoredThisSwing)
            {
                if (maxPositiveAngle >= swingAngleB)
                {
                    RegisterSwing(maxPositiveAngle);
                }

                alreadyScoredThisSwing = true;
            }

            swingStarted = false;
            maxPositiveAngle = 0f;
        }
    }

    private void RegisterSwing(float maxAngle)
    {
        float multiplier = 1f;

        // BONUS ONLY FROM POSITIVE ANGLE
        if (maxAngle > swingAngleB)
        {
            float extra = maxAngle - swingAngleB;

            multiplier += extra * extra * extraMultiplierScale;
        }

        int awarded = Mathf.RoundToInt(
            swingPoints * multiplier
        );

        score += awarded;

        Debug.Log(
            $"[SWING] positiveMax={maxAngle:F2} " +
            $"multiplier={multiplier:F2} " +
            $"awarded={awarded} " +
            $"total={score}"
        );

        if (cislaDisplay != null)
        {
            cislaDisplay.Write(score);
        }

        if (scorePopup != null)
        {
            scorePopup.Show(awarded);
        }
    }

    private void CheckFall()
    {
        float angle = Mathf.Abs(GetSignedAngle());

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

        swingStarted = false;
        alreadyScoredThisSwing = false;

        maxPositiveAngle = 0f;

        chairRigidbody.linearVelocity = Vector3.zero;
        chairRigidbody.angularVelocity = Vector3.zero;

        chairRigidbody.transform.position = startPos;
        chairRigidbody.transform.rotation = startRot;

        if (cislaDisplay != null)
        {
            cislaDisplay.Write(score);
        }

        isFalling = false;

        Debug.Log("[RESET] Game reset hotový");
    }

    private void DebugAngles()
    {
        if (!debugAngles)
            return;

        debugTimer += Time.fixedDeltaTime;

        if (debugTimer >= debugInterval)
        {
            debugTimer = 0f;

            float angle = GetSignedAngle();

            Debug.Log(
                $"[DEBUG] SignedAngle={angle:F2} | " +
                $"PositiveMax={maxPositiveAngle:F2} | " +
                $"AngularVel={chairRigidbody.angularVelocity}"
            );
        }
    }
}
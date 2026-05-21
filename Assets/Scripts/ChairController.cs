using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChairController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;
    public float maxAngle = 60f;
    // new: minimum allowed angle (prevents rotating "pod podlahu")
    public float minAngle = 0f;
    public float smoothing = 5f;

    [Header("Input")]
    [Tooltip("Small deadzone to ignore tiny input drift")]
    public float inputDeadzone = 0.05f;

    [Header("Input Actions (References)")]
    public InputActionReference rotateForwardRef;
    public InputActionReference rotateBackwardRef;

    [Header("Input Actions (Properties)")]
    public InputActionProperty rotateForwardAction;
    public InputActionProperty rotateBackwardAction;

    [Header("3D Buttons")]
    public XRSimpleInteractable buttonForward;
    public XRSimpleInteractable buttonBackward;

    [Header("Pivot (optional)")]
    [Tooltip("Pokud chcete rotovat židli kolem urèité pozice, pøiøaïte sem Transform. Pokud je null, použije se lokální rotace židle.")]
    public Transform rotationPivot;

    // new: disable rotation in one direction if needed
    [Header("Direction Constraints")]
    [Tooltip("Povolit rotaci dopøedu (kladný smìr). Pokud false, pohyb dopøedu bude ignorován.")]
    public bool allowForward = true;
    [Tooltip("Povolit rotaci dozadu (záporný smìr). Pokud false, pohyb dozadu bude ignorován.")]
    public bool allowBackward = true;

    // new: falling behaviour
    [Header("Falling (unstable) settings")]
    [Tooltip("Pokud absolutní úhel dosáhne této hodnoty, židle pøejde do nekontrolovatelného pádu.")]
    public float fallThresholdAngle = 80f;
    [Tooltip("Doba (s), po kterou musíte držet opaèné tlaèítko pro zvednutí židle po pádu.")]
    public float recoverHoldTime = 0.5f;

    [Header("Auto-backward (self-correct) settings")]
    [Tooltip("Úhel (v°) za kterým zaène židle sama vracet dozadu (pozitivní smìr). Pokud je 0 = disabled.")]
    public float autoBackwardStartAngle = 45f;
    [Tooltip("Síla (škálovaný faktor) samovolného vracení. Vìtší = rychlejší návrat.")]
    public float autoBackwardStrength = 1f;
    [Tooltip("Povolit automatické vracení dozadu, když je úhel vìtší než hranice.")]
    public bool enableAutoBackward = true;

    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private bool isForwardPressed = false;
    private bool isBackwardPressed = false;

    // track last applied angle to apply incremental RotateAround when using pivot
    private float lastAppliedAngle = 0f;

    // falling state
    private bool isFalling = false;
    private Rigidbody fallingRb = null;
    private float recoverTimer = 0f;

    void OnEnable()
    {
        if (rotateForwardRef != null && rotateForwardRef.action != null) rotateForwardRef.action.Enable();
        if (rotateBackwardRef != null && rotateBackwardRef.action != null) rotateBackwardRef.action.Enable();

        if (rotateForwardAction.action != null) rotateForwardAction.action.Enable();
        if (rotateBackwardAction.action != null) rotateBackwardAction.action.Enable();
    }

    // Keep inspector values sane: ensure minAngle <= maxAngle and positive smoothing
    void OnValidate()
    {
        if (minAngle > maxAngle)
        {
            float tmp = minAngle;
            minAngle = maxAngle;
            maxAngle = tmp;
            Debug.LogWarning($"ChairController: swapped minAngle/maxAngle because minAngle was greater than maxAngle.");
        }

        smoothing = Mathf.Max(0.0001f, smoothing);
        fallThresholdAngle = Mathf.Abs(fallThresholdAngle);
        inputDeadzone = Mathf.Max(0f, inputDeadzone);
    }

    void Start()
    {
        if (buttonForward != null)
        {
            buttonForward.hoverEntered.AddListener(_ => isForwardPressed = true);
            buttonForward.hoverExited.AddListener(_ => isForwardPressed = false);
            buttonForward.selectEntered.AddListener(_ => isForwardPressed = true);
            buttonForward.selectExited.AddListener(_ => isForwardPressed = false);
        }

        if (buttonBackward != null)
        {
            buttonBackward.hoverEntered.AddListener(_ => isBackwardPressed = true);
            buttonBackward.hoverExited.AddListener(_ => isBackwardPressed = false);
            buttonBackward.selectEntered.AddListener(_ => isBackwardPressed = true);
            buttonBackward.selectExited.AddListener(_ => isBackwardPressed = false);
        }

        // Initialize lastAppliedAngle tak, aby nebyl náhlý skok pøi prvním Update
        lastAppliedAngle = currentAngle;
    }

    void Update()
    {
        // when falling, ignore normal rotation inputs except for recovery hold
        if (isFalling)
        {
            HandleRecovery();
            return;
        }

        float forwardValue = GetInputValue(rotateForwardRef, rotateForwardAction) + (isForwardPressed ? 1f : 0f);
        float backwardValue = GetInputValue(rotateBackwardRef, rotateBackwardAction) + (isBackwardPressed ? 1f : 0f);

        float move = forwardValue - backwardValue;

        // enforce direction constraints
        if (move > 0f && !allowForward) move = 0f;
        if (move < 0f && !allowBackward) move = 0f;

        // apply deadzone to avoid tiny drifts
        if (Mathf.Abs(move) > inputDeadzone)
        {
            targetAngle += move * rotationSpeed * Time.deltaTime;
        }

        // use effective min/max so inspector mistakes (min>max) don't break runtime
        float effectiveMin = Mathf.Min(minAngle, maxAngle);
        float effectiveMax = Mathf.Max(minAngle, maxAngle);

        // if user left minAngle at 0 but allowed backward motion, treat limits symmetrically
        if (allowBackward && effectiveMin >= 0f)
        {
            effectiveMin = -effectiveMax;
        }

        // Auto-backward: when past configured start angle, apply a pulling force back
        if (enableAutoBackward && autoBackwardStartAngle > 0f)
        {
            // decide if we should apply auto-backward based on current targetAngle and whether backward input is held
            if (targetAngle > autoBackwardStartAngle)
            {
                // check if backward is being held (if yes, auto pull still combines with input as requested)
                bool backwardHeld = (backwardValue > 0.01f);

                // compute pull magnitude proportional to how far past the threshold we are
                float over = targetAngle - autoBackwardStartAngle;
                float pull = autoBackwardStrength * over;

                // apply pull only when not explicitly prevented; it always adds to the net movement
                // pull acts in negative direction (toward decreasing angle)
                targetAngle -= pull * Time.deltaTime * (backwardHeld ? 1f : 1f);
            }
            else if (targetAngle < -autoBackwardStartAngle)
            {
                // symmetric behavior for negative side: pull toward positive (forward)
                bool forwardHeld = (forwardValue > 0.01f);
                float over = -autoBackwardStartAngle - targetAngle; // how far beyond negative threshold
                float pull = autoBackwardStrength * over;
                targetAngle += pull * Time.deltaTime * (forwardHeld ? 1f : 1f);
            }
        }

        // clamp by configured min/max angles to prevent going "pod podlahu"
        float clamped = Mathf.Clamp(targetAngle, effectiveMin, effectiveMax);
        bool hitLimit = !Mathf.Approximately(clamped, targetAngle);
        targetAngle = clamped;

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothing);

        // if we've hit a hard limit, snap currentAngle to avoid LERP oscillation at extremes
        if (hitLimit)
        {
            currentAngle = targetAngle;
            lastAppliedAngle = currentAngle;
        }

        // check falling threshold using the actual currentAngle (smoothed) to avoid false triggers
        if (Mathf.Abs(currentAngle) >= fallThresholdAngle)
        {
            StartFalling();
            return;
        }

        if (rotationPivot != null)
        {
            // Aplikuj inkrementální rotaci kolem zvoleného pivotu (world axis = pivot.right)
            float delta = currentAngle - lastAppliedAngle;
            if (Mathf.Abs(delta) > 0.0001f)
            {
                Vector3 axis = rotationPivot.right;
                transform.RotateAround(rotationPivot.position, axis, delta);
            }
            lastAppliedAngle = currentAngle;
        }
        else
        {
            // Pùvodní chování: lokální rotace židle
            transform.localRotation = Quaternion.Euler(currentAngle, 0, 0);
            lastAppliedAngle = currentAngle;
        }

        // Debug log to console if it moves
        if (Mathf.Abs(move) > 0.01f)
        {
            // Debug.Log($"Chair Moving: {move}, Angle: {currentAngle}");
        }
    }

    private void HandleRecovery()
    {
        // Determine which input is the opposite direction required to lift the chair
        bool oppositeHeld = false;
        // if targetAngle positive, need backward to recover; if negative, need forward
        // use the current processed input values (buttons + actions)
        float forwardValue = GetInputValue(rotateForwardRef, rotateForwardAction) + (isForwardPressed ? 1f : 0f);
        float backwardValue = GetInputValue(rotateBackwardRef, rotateBackwardAction) + (isBackwardPressed ? 1f : 0f);

        if (targetAngle > 0f)
        {
            oppositeHeld = backwardValue > 0.1f;
        }
        else if (targetAngle < 0f)
        {
            oppositeHeld = forwardValue > 0.1f;
        }

        if (oppositeHeld)
        {
            recoverTimer += Time.deltaTime;
            if (recoverTimer >= recoverHoldTime)
            {
                RecoverFromFall();
            }
        }
        else
        {
            recoverTimer = 0f;
        }
    }

    private void StartFalling()
    {
        isFalling = true;

        // add Rigidbody if none - make it non-kinematic to allow gravity to act
        fallingRb = GetComponent<Rigidbody>();
        if (fallingRb == null)
        {
            fallingRb = gameObject.AddComponent<Rigidbody>();
            // optionally set mass and constraints to reasonable defaults
            fallingRb.mass = 5f;
        }
        fallingRb.isKinematic = false;

        // allow physics to take over; we don't update rotation while falling
    }

    private void RecoverFromFall()
    {
        // remove Rigidbody if we added it
        if (fallingRb != null)
        {
            // if Rigidbody was added by this script (we can't know for sure), try to remove it
            // to be safe, set to kinematic and then destroy
            fallingRb.isKinematic = true;
            Destroy(fallingRb);
            fallingRb = null;
        }

        // reset state and snap chair upright (zero angle)
        isFalling = false;
        recoverTimer = 0f;
        targetAngle = 0f;
        currentAngle = 0f;
        lastAppliedAngle = 0f;

        // snap rotation to upright
        transform.localRotation = Quaternion.identity;
    }

    private float GetInputValue(InputActionReference reference, InputActionProperty property)
    {
        float val = 0f;
        if (reference != null && reference.action != null)
        {
            val = reference.action.ReadValue<float>();
        }

        if (val < 0.01f && property.action != null)
        {
            val = property.action.ReadValue<float>();
        }

        return val;
    }
}
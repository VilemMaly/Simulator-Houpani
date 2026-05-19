using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChairController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;
    public float maxAngle = 60f;
    public float smoothing = 5f;

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

    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private bool isForwardPressed = false;
    private bool isBackwardPressed = false;

    // track last applied angle to apply incremental RotateAround when using pivot
    private float lastAppliedAngle = 0f;

    void OnEnable()
    {
        if (rotateForwardRef != null && rotateForwardRef.action != null) rotateForwardRef.action.Enable();
        if (rotateBackwardRef != null && rotateBackwardRef.action != null) rotateBackwardRef.action.Enable();

        if (rotateForwardAction.action != null) rotateForwardAction.action.Enable();
        if (rotateBackwardAction.action != null) rotateBackwardAction.action.Enable();
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
        float forwardValue = GetInputValue(rotateForwardRef, rotateForwardAction) + (isForwardPressed ? 1f : 0f);
        float backwardValue = GetInputValue(rotateBackwardRef, rotateBackwardAction) + (isBackwardPressed ? 1f : 0f);

        float move = forwardValue - backwardValue;

        if (Mathf.Abs(move) > 0.01f)
        {
            targetAngle += move * rotationSpeed * Time.deltaTime;
        }

        targetAngle = Mathf.Clamp(targetAngle, -maxAngle, maxAngle);
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothing);

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
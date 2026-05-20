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
    [Tooltip("Doporučeno: XRI RightHand Interaction/PrimaryButton (A)")]
    public InputActionProperty forwardAction;

    [Tooltip("Doporučeno: XRI RightHand Interaction/SecondaryButton (B)")]
    public InputActionProperty backwardAction;

    private bool isFalling = false;

    private void OnEnable()
    {
        // DŮLEŽITÉ: Akce se musí povolit, aby začaly posílat data
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
        if (chairRigidbody == null)
            chairRigidbody = GetComponentInChildren<Rigidbody>();

        if (centerOfMassTarget != null && chairRigidbody != null)
        {
            chairRigidbody.centerOfMass = chairRigidbody.transform.InverseTransformPoint(centerOfMassTarget.position);
            Debug.Log($"[ChairSystem] Těžiště nastaveno na: {chairRigidbody.centerOfMass}");
        }
        else
        {
            Debug.LogWarning("[ChairSystem] Chybí Rigidbody nebo CenterOfMassTarget!");
        }
    }

    void FixedUpdate()
    {
        if (chairRigidbody == null || isFalling) return;

        // Načtení hodnot (0 = puštěno, 1 = zmáčknuto)
        float forward = forwardAction.action?.ReadValue<float>() ?? 0f;
        float backward = backwardAction.action?.ReadValue<float>() ?? 0f;

        // DEBUG LOG: Odkomentujte řádek níže, pokud chcete vidět vstupy v konzoli
        // if (forward > 0 || backward > 0) Debug.Log($"Forward: {forward}, Backward: {backward}");

        float moveInput = forward - backward;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            // Aplikujeme sílu pro houpání
            chairRigidbody.AddRelativeTorque(Vector3.right * moveInput * torqueStrength, ForceMode.Acceleration);
        }

        CheckForFall();
    }

    private void CheckForFall()
    {
        // Výpočet úhlu vůči svislé ose
        float angle = Vector3.Angle(chairRigidbody.transform.up, Vector3.up);

        if (angle > fallThreshold && !isFalling)
        {
            isFalling = true;
            Debug.LogError($"[ChairSystem] PÁD! Židle se převrátila. Úhel: {angle:F1}°");

            // Tady by mohl přijít haptický impulz
        }
    }

    public void ResetChair()
    {
        isFalling = false;
        chairRigidbody.transform.localRotation = Quaternion.identity;
        chairRigidbody.linearVelocity = Vector3.zero;
        chairRigidbody.angularVelocity = Vector3.zero;
        Debug.Log("[ChairSystem] Židle byla resetována.");
    }
}
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class RelayVRMenu : MonoBehaviour
{
    [Header("Button Colliders")]
    public Collider hostButton;
    public Collider joinButton;

    [Header("Code Display")]
    public Cisla codeDisplay;

    [Header("Settings")]
    public string handTag = "hand";

    private bool typingJoinCode = false;
    private string currentJoinCode = "";

    private TouchScreenKeyboard mobileKeyboard;

    // --- VR debounce protection ---
    private bool canPressJoin = true;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void Update()
    {
        if (!typingJoinCode)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR

        if (mobileKeyboard != null)
        {
            currentJoinCode = mobileKeyboard.text.ToUpper();

            codeDisplay.Write(currentJoinCode);

            if (mobileKeyboard.status == TouchScreenKeyboard.Status.Done)
            {
                typingJoinCode = false;

                if (!string.IsNullOrEmpty(currentJoinCode))
                {
                    JoinRelay(currentJoinCode);
                }
            }
        }

#else

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        foreach (KeyControl key in keyboard.allKeys)
        {
            if (!key.wasPressedThisFrame)
                continue;

            string keyName = key.displayName;

            // BACKSPACE
            if (key == keyboard.backspaceKey)
            {
                if (currentJoinCode.Length > 0)
                {
                    currentJoinCode =
                        currentJoinCode.Substring(0, currentJoinCode.Length - 1);

                    codeDisplay.Write(currentJoinCode);
                }

                continue;
            }

            // ENTER
            if (key == keyboard.enterKey || key == keyboard.numpadEnterKey)
            {
                typingJoinCode = false;

                if (!string.IsNullOrEmpty(currentJoinCode))
                {
                    JoinRelay(currentJoinCode);
                }

                continue;
            }

            // LETTERS + NUMBERS
            if (!string.IsNullOrEmpty(keyName) && keyName.Length == 1)
            {
                char c = keyName[0];

                if (char.IsLetterOrDigit(c))
                {
                    currentJoinCode += char.ToUpper(c);
                    codeDisplay.Write(currentJoinCode);
                }
            }
        }

#endif
    }

    // =========================
    // VR TRIGGER LOGIC
    // =========================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(handTag))
            return;

        // JOIN BUTTON
        if (other == joinButton && canPressJoin)
        {
            canPressJoin = false;
            PressJoin();
        }

        // HOST BUTTON
        if (other == hostButton)
        {
            PressHost();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(handTag))
            return;

        if (other == joinButton)
        {
            canPressJoin = true;
        }
    }

    // =========================
    // BUTTON ACTIONS
    // =========================

    public void PressHost()
    {
        StartCoroutine(StartHostRoutine());
    }

    public void PressJoin()
    {
        StartJoinInput();
    }

    private IEnumerator StartHostRoutine()
    {
        yield return CreateRelay();
    }

    private async Awaitable CreateRelay()
    {
        try
        {
            Allocation allocation =
                await RelayService.Instance.CreateAllocationAsync(1);

            string joinCode =
                await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            codeDisplay.Write(joinCode);

            RelayServerData relayServerData =
                AllocationUtils.ToRelayServerData(allocation, "dtls");

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            Debug.Log("HOST STARTED: " + joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private void StartJoinInput()
    {
        typingJoinCode = true;
        currentJoinCode = "";
        codeDisplay.Write("");

#if UNITY_ANDROID && !UNITY_EDITOR

        mobileKeyboard = TouchScreenKeyboard.Open(
            "",
            TouchScreenKeyboardType.Default,
            false,
            false,
            true,
            false,
            "ENTER CODE"
        );

#endif
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation allocation =
                await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData =
                AllocationUtils.ToRelayServerData(allocation, "dtls");

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            Debug.Log("JOINED: " + joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}
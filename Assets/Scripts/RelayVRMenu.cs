using System.Collections;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class RelayVRMenu : MonoBehaviour
{
    [Header("Button Colliders")]
    public Collider hostButton;
    public Collider joinButton;

    [Header("IP Display")]
    public Cisla codeDisplay;

    [Header("Settings")]
    public string handTag = "hand";

    [Header("Player Prefab")]
    public NetworkObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Editor Only Players (destroy after connect)")]
    public GameObject[] editorPlayers;

    [Header("Network")]
    public ushort port = 7777;

    private int nextSpawnIndex = 0;

    private bool typingJoinCode = false;
    private string currentJoinCode = "";

    private TouchScreenKeyboard mobileKeyboard;

    private bool canPressJoin = true;

    // ==================================================
    // START
    // ==================================================

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // ==================================================
    // CLIENT CONNECTED
    // ==================================================

    private void OnClientConnected(ulong clientId)
    {
        RemoveEditorPlayers();

        if (!NetworkManager.Singleton.IsServer)
            return;

        SpawnPlayer(clientId);
    }

    // ==================================================
    // CUSTOM PLAYER SPAWN
    // ==================================================

    private void SpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab missing!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        Transform spawnPoint =
            spawnPoints[nextSpawnIndex % spawnPoints.Length];

        nextSpawnIndex++;

        NetworkObject playerInstance = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        playerInstance.SpawnAsPlayerObject(clientId);

        Debug.Log(
            $"Spawned player {clientId} at {spawnPoint.position}"
        );
    }

    // ==================================================
    // REMOVE EDITOR PLAYERS
    // ==================================================

    private void RemoveEditorPlayers()
    {
        if (editorPlayers == null)
            return;

        for (int i = 0; i < editorPlayers.Length; i++)
        {
            if (editorPlayers[i] != null)
            {
                Destroy(editorPlayers[i]);
            }
        }
    }

    // ==================================================
    // INPUT SYSTEM
    // ==================================================

    private void Update()
    {
        if (!typingJoinCode)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR

        if (mobileKeyboard != null)
        {
            currentJoinCode = mobileKeyboard.text;

            codeDisplay.Write(currentJoinCode);

            if (mobileKeyboard.status == TouchScreenKeyboard.Status.Done)
            {
                typingJoinCode = false;

                if (!string.IsNullOrEmpty(currentJoinCode))
                    JoinServer(currentJoinCode);
            }
        }

#else

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        // =========================================
        // BACKSPACE
        // =========================================

        if (keyboard.backspaceKey.wasPressedThisFrame)
        {
            if (currentJoinCode.Length > 0)
            {
                currentJoinCode =
                    currentJoinCode.Substring(
                        0,
                        currentJoinCode.Length - 1
                    );

                codeDisplay.Write(currentJoinCode);
            }
        }

        // =========================================
        // ENTER
        // =========================================

        if (keyboard.enterKey.wasPressedThisFrame ||
            keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            Debug.Log(currentJoinCode);

            typingJoinCode = false;

            if (!string.IsNullOrEmpty(currentJoinCode))
                JoinServer(currentJoinCode);
        }

        // =========================================
        // LETTERS
        // =========================================

        CheckKey(keyboard.aKey, 'A');
        CheckKey(keyboard.bKey, 'B');
        CheckKey(keyboard.cKey, 'C');
        CheckKey(keyboard.dKey, 'D');
        CheckKey(keyboard.eKey, 'E');
        CheckKey(keyboard.fKey, 'F');
        CheckKey(keyboard.gKey, 'G');
        CheckKey(keyboard.hKey, 'H');
        CheckKey(keyboard.iKey, 'I');
        CheckKey(keyboard.jKey, 'J');
        CheckKey(keyboard.kKey, 'K');
        CheckKey(keyboard.lKey, 'L');
        CheckKey(keyboard.mKey, 'M');
        CheckKey(keyboard.nKey, 'N');
        CheckKey(keyboard.oKey, 'O');
        CheckKey(keyboard.pKey, 'P');
        CheckKey(keyboard.qKey, 'Q');
        CheckKey(keyboard.rKey, 'R');
        CheckKey(keyboard.sKey, 'S');
        CheckKey(keyboard.tKey, 'T');
        CheckKey(keyboard.uKey, 'U');
        CheckKey(keyboard.vKey, 'V');
        CheckKey(keyboard.wKey, 'W');
        CheckKey(keyboard.xKey, 'X');
        CheckKey(keyboard.yKey, 'Y');
        CheckKey(keyboard.zKey, 'Z');

        // =========================================
        // TOP ROW NUMBERS
        // =========================================

        CheckKey(keyboard.digit0Key, '0');
        CheckKey(keyboard.digit1Key, '1');
        CheckKey(keyboard.digit2Key, '2');
        CheckKey(keyboard.digit3Key, '3');
        CheckKey(keyboard.digit4Key, '4');
        CheckKey(keyboard.digit5Key, '5');
        CheckKey(keyboard.digit6Key, '6');
        CheckKey(keyboard.digit7Key, '7');
        CheckKey(keyboard.digit8Key, '8');
        CheckKey(keyboard.digit9Key, '9');

        // =========================================
        // DOT
        // =========================================

        if (keyboard.periodKey.wasPressedThisFrame)
        {
            currentJoinCode += ".";
            codeDisplay.Write(currentJoinCode);
        }

        // =========================================
        // NUMPAD
        // =========================================

        CheckKey(keyboard.numpad0Key, '0');
        CheckKey(keyboard.numpad1Key, '1');
        CheckKey(keyboard.numpad2Key, '2');
        CheckKey(keyboard.numpad3Key, '3');
        CheckKey(keyboard.numpad4Key, '4');
        CheckKey(keyboard.numpad5Key, '5');
        CheckKey(keyboard.numpad6Key, '6');
        CheckKey(keyboard.numpad7Key, '7');
        CheckKey(keyboard.numpad8Key, '8');
        CheckKey(keyboard.numpad9Key, '9');

#endif
    }

    private void CheckKey(KeyControl key, char c)
    {
        if (!key.wasPressedThisFrame)
            return;

        currentJoinCode += c;

        codeDisplay.Write(currentJoinCode);
    }

    // ==================================================
    // VR TRIGGERS
    // ==================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(handTag))
            return;

        if (other == joinButton && canPressJoin)
        {
            canPressJoin = false;
            PressJoin();
        }

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
            canPressJoin = true;
    }

    // ==================================================
    // BUTTON ACTIONS
    // ==================================================

    public void PressHost()
    {
        StartHost();
    }

    public void PressJoin()
    {
        StartJoinInput();
    }

    // ==================================================
    // HOST
    // ==================================================

    private void StartHost()
    {
        string localIP = GetLocalIPAddress();

        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(
            "0.0.0.0",
            port
        );

        NetworkManager.Singleton.StartHost();

        codeDisplay.Write(localIP);

        Debug.Log("HOST STARTED");
        Debug.Log("IP: " + localIP);
    }

    // ==================================================
    // JOIN INPUT
    // ==================================================

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
            "ENTER IP ADDRESS"
        );

#endif
    }

    // ==================================================
    // CLIENT JOIN
    // ==================================================

    private void JoinServer(string ip)
    {
        try
        {
            ip = ip.Trim();

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetConnectionData(
                ip,
                port
            );

            NetworkManager.Singleton.StartClient();

            Debug.Log("JOINING: " + ip);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    // ==================================================
    // GET LOCAL IP
    // ==================================================

    private string GetLocalIPAddress()
    {
        string localIP = "127.0.0.1";

        IPHostEntry host =
            Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }

        return localIP;
    }
}
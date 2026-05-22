using UnityEngine;
using System.Collections;

public class PaperBallSpawn : MonoBehaviour
{
    [Header("Ball settings")]
    public GameObject paperBall;

    [Header("Respawn delay")]
    public float respawnDelay = 5f;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private bool ballInside = true;
    private bool respawnRunning = false;

    void Start()
    {
        if (paperBall == null)
        {
            Debug.LogError("PaperBall není nastavený.");
            return;
        }

        spawnPosition = paperBall.transform.position;
        spawnRotation = paperBall.transform.rotation;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == paperBall && !respawnRunning)
        {
            ballInside = false;
            StartCoroutine(RespawnBall());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == paperBall)
        {
            ballInside = true;
        }
    }

    IEnumerator RespawnBall()
    {
        respawnRunning = true;

        yield return new WaitForSeconds(respawnDelay);

        if (!ballInside)
        {
            GameObject newBall = Instantiate(
                paperBall,
                spawnPosition,
                spawnRotation
            );

            // NOVÝ míček je teď sledovaný objekt
            paperBall = newBall;

            ballInside = true;
        }

        respawnRunning = false;
    }
}
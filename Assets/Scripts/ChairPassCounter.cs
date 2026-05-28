using UnityEngine;

public class ChairPassCounter : MonoBehaviour
{
    [Header("Display skóre")]
    public Cisla scoreDisplay;

    [Header("Display odpočtu")]
    public Cisla timerDisplay;

    [Header("Tag židle")]
    [SerializeField]
    private string chairTag = "Zidle";

    [Header("Odpočet v sekundách")]
    public float countdownSeconds = 20f;

    private int count = 0;

    // průchod
    private bool passedFirst = false;
    private GameObject trackedChair = null;

    // timer
    private bool timerRunning = false;
    private float timerRemaining = 0f;

    private void Start()
    {
        if (scoreDisplay != null)
            scoreDisplay.Write(0);

        if (timerDisplay != null)
            timerDisplay.Write(Mathf.CeilToInt(countdownSeconds * 1000f));
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        timerRemaining -= Time.deltaTime;

        if (timerRemaining < 0f)
            timerRemaining = 0f;

        // zobrazení v milisekundách
        int milliseconds = Mathf.CeilToInt(timerRemaining * 1000f);

        if (timerDisplay != null)
            timerDisplay.Write(milliseconds);

        if (timerRemaining <= 0f)
        {
            timerRunning = false;

            passedFirst = false;
            trackedChair = null;
        }
    }

    public void HitFirst(Collider other)
    {
        GameObject rootObj = other.transform.root.gameObject;

        if (!rootObj.CompareTag(chairTag))
            return;

        trackedChair = rootObj;
        passedFirst = true;

        // start odpočtu
        timerRemaining = countdownSeconds;
        timerRunning = true;
    }

    public void HitSecond(Collider other)
    {
        GameObject rootObj = other.transform.root.gameObject;

        if (!rootObj.CompareTag(chairTag))
            return;

        if (!passedFirst)
            return;

        if (trackedChair != rootObj)
            return;

        // timer musí ještě běžet
        if (!timerRunning)
            return;

        count++;

        if (scoreDisplay != null)
            scoreDisplay.Write(count);

        // reset průchodu
        passedFirst = false;
        trackedChair = null;

        // zastavení timeru
        timerRunning = false;

        if (timerDisplay != null)
            timerDisplay.Write(0);
    }

    public void ResetState()
    {
        passedFirst = false;
        trackedChair = null;

        timerRunning = false;

        if (timerDisplay != null)
            timerDisplay.Write(0);
    }
}
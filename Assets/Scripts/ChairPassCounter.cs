using UnityEngine;

public class ChairPassCounter : MonoBehaviour
{
    [Header("Display skóre")]
    public Cisla scoreDisplay;

    [Header("Display času")]
    public Cisla timerDisplay;

    [Header("Tag židle")]
    [SerializeField]
    private string chairTag = "Zidle";

    private int count = 0;

    // stav průchodu
    private bool waitingForSecond = false;
    private bool waitingForReturn = false;

    private GameObject trackedChair = null;

    // měření času
    private float timer = 0f;
    private bool timerRunning = false;

    private void Start()
    {
        if (scoreDisplay != null)
            scoreDisplay.Write(0);

        if (timerDisplay != null)
            timerDisplay.Write(0);
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        timer += Time.deltaTime;

        int milliseconds = Mathf.CeilToInt(timer * 1000f);

        if (timerDisplay != null)
            timerDisplay.Write(milliseconds);
    }

    // první collider (START)
    public void HitFirst(Collider other)
    {
        GameObject rootObj = other.transform.root.gameObject;

        if (!rootObj.CompareTag(chairTag))
            return;

        // start měření
        trackedChair = rootObj;
        waitingForSecond = true;
        waitingForReturn = false;

        timer = 0f;
        timerRunning = true;
    }

    // druhý collider (MIDPOINT)
    public void HitSecond(Collider other)
    {
        GameObject rootObj = other.transform.root.gameObject;

        if (!rootObj.CompareTag(chairTag))
            return;

        if (!waitingForSecond)
            return;

        if (trackedChair != rootObj)
            return;

        waitingForSecond = false;
        waitingForReturn = true;
    }

    // návrat zpět do prvního collideru (END)
    public void HitReturn(Collider other)
    {
        GameObject rootObj = other.transform.root.gameObject;

        if (!rootObj.CompareTag(chairTag))
            return;

        if (!waitingForReturn)
            return;

        if (trackedChair != rootObj)
            return;

        // STOP TIMER
        timerRunning = false;

        count++;

        if (scoreDisplay != null)
            scoreDisplay.Write(count);

        if (timerDisplay != null)
            timerDisplay.Write(Mathf.CeilToInt(timer * 1000f));

        // reset
        waitingForSecond = false;
        waitingForReturn = false;
        trackedChair = null;
        timer = 0f;
    }

    public void ResetState()
    {
        waitingForSecond = false;
        waitingForReturn = false;
        trackedChair = null;

        timerRunning = false;
        timer = 0f;

        if (timerDisplay != null)
            timerDisplay.Write(0);
    }
}
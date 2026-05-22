using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public Rigidbody chairRb;

    private int score = 0;
    private bool gameOver = false;

    private bool wasLeaningBack = false;

    void Update()
    {
        if (!gameOver)
        {
            float zRotation = chairRb.transform.rotation.eulerAngles.z;

            // převod rotace
            if (zRotation > 180)
                zRotation -= 360;

            // když se hráč zhoupne dozadu
            if (zRotation < -15 && !wasLeaningBack)
            {
                score++;
                wasLeaningBack = true;
            }

            // reset pro další zhoupnutí
            if (zRotation > -5)
            {
                wasLeaningBack = false;
            }

            scoreText.text = "Score: " + score;

            // pád
            if (Mathf.Abs(zRotation) > 70)
            {
                gameOver = true;

                scoreText.text =
                    "Game Over\nScore: " + score;
            }
        }
    }
}
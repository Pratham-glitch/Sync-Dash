using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text scoreText;
    public TMP_Text ghostCcoreText;

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
        if (ghostCcoreText != null)
            ghostCcoreText.text = "Score: " + score.ToString();

    }
    public void UpdateScore_Bot(int score)
    {
        if (ghostCcoreText != null)
            ghostCcoreText.text = "Score: " + score.ToString();
    }

    /*public void UpdateSpeed(float speed)
     {
         if (speedText != null)
             speedText.text = "Speed: " + speed.ToString("F1");
     }*/
}
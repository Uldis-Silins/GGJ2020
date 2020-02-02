using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI opponentScoreText;

    public Image scoreFeedbackImage;

    public Sprite correctOpenCard;
    public Sprite failedOpenCard;

    public Button myTurnButton;

    private readonly float m_scoreFeedbackTime = 1f;
    private float m_scoreFeedbackTimer;

    private void Update()
    {
        if(m_scoreFeedbackTimer <= 0f)
        {
            scoreFeedbackImage.gameObject.SetActive(false);
        }
        else
        {
            m_scoreFeedbackTimer -= Time.deltaTime;
        }
    }

    public void OnQuitButtonClick()
    {
        Application.Quit();
    }

    public void SetPlayerScoreText(int score)
    {
        playerScoreText.text = "P1: " + score.ToString();
    }

    public void SetOpponentScoreText(int score)
    {
        opponentScoreText.text = "P2: " + score.ToString();
    }

    public void OnMyTurnClick()
    {
        myTurnButton.gameObject.SetActive(false);
    }

    public void OnOpenCard(bool isCorrect)
    {
        scoreFeedbackImage.sprite = isCorrect ? correctOpenCard : failedOpenCard;
        scoreFeedbackImage.gameObject.SetActive(true);
        m_scoreFeedbackTimer = m_scoreFeedbackTime;
    }
}

using UnityEngine;
using TMPro; // Include TextMeshPro namespace

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;  // Reference to the TextMeshPro UI object
    private float playerScore = 0f;

    // Start is called before the first frame update
    void Start()
    {
        UpdateScoreText();
    }

    private void Update()
    {
    }

    // This method will be called to add points and update the score
    public void AddScore(float scoreToAdd)
    {
        playerScore += scoreToAdd;
        UpdateScoreText();
    }

    // Update the TextMeshPro UI text with the current scorea
    void UpdateScoreText()
    {
        scoreText.text = "Score: " + playerScore.ToString();
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameResultManager : MonoBehaviour
{
    public GameObject winScreen;
    public GameObject loseScreen;
    public Button nextRoundButton;
    public Button restartGameButton;

    [Header("Card Group")]
    public Transform playingCardGroup; // Card container reference

    private bool gameEnded = false;

    private void Start()
    {
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        nextRoundButton.onClick.AddListener(NextRound);
        restartGameButton.onClick.AddListener(RestartGame);
    }

    public void CheckGameState()
    {
        Debug.Log("Checking game state...");
        
        // Exit if the game already ended
        if (gameEnded) {
            Debug.Log("Game already ended, ignoring check");
            return;
        }

        int score = LevelManager.Instance.pointsScored;
        int requiredScore = LevelManager.Instance.scoreRequiredForLevel;
        int remainingHands = LevelManager.Instance.handsRemaining;
        bool finalScoreEvaluated = LevelManager.Instance.awaitingFinalScoreEvaluation;

        Debug.Log($"Current score: {score}, Required score: {requiredScore}, Remaining hands: {remainingHands}");

        // Win condition: Player has reached or exceeded the required score
        if (score >= requiredScore)
        {
            Debug.Log("Win condition met: Score is sufficient!");
            ShowWinScreen();
            gameEnded = true;
            return;
        }

        // Lose condition: No more hands remaining but score is insufficient
        if (remainingHands <= 0 && score < requiredScore && finalScoreEvaluated)
        {
            Debug.Log("Lose condition met: No more hands and score is insufficient");
            ShowLoseScreen();
            gameEnded = true;
            LevelManager.Instance.awaitingFinalScoreEvaluation = false;
            return;
        }

        // Game continues
        Debug.Log("Game continuing - no win/lose condition met yet");
    }

    void ShowWinScreen()
    {
        Debug.Log("Showing win screen");
        winScreen.SetActive(true);
    }

    void ShowLoseScreen()
    {
        Debug.Log("Showing lose screen");
        loseScreen.SetActive(true);
    }

    public void NextRound()
    {
        LevelManager.Instance.NextLevel();
        Destroy(LevelManager.Instance.gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        LevelManager.Instance.ResetGame();
        Destroy(LevelManager.Instance.gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
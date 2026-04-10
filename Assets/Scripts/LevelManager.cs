using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;  // Singleton instance

    [Header("Level Settings")]
    public int currentRound = 1; //for debugging and easy checking.
    public int currentAnte = 1;
    public int scoreRequiredForLevel;
    public int handsRemaining = 5;
    public int discardsRemaining = 4;
    public int pointsScored = 0;
    public int handScore = 0;
    public float handMultiplier = 0;
    public string pokerHandSelected;
    public bool awaitingFinalScoreEvaluation = false;
    
    public Text scoreRequiredText;
    public Text scoreCollectedText;
    public Text handsRemainingText;
    public Text discardsRemainingText;
    public Text anteText;
    public Text roundText;
    public Text handScoreText;
    public Text handMultiplierText;
    public Text pokerHandSelectedText;

    public GameResultManager resultManager;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of LevelManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (resultManager == null)
        {
            resultManager = FindFirstObjectByType<GameResultManager>();
        }
    }

    void Start()
    {
        UpdateLevelScore();
        UpdateHandsRemaining(handsRemaining);
        SetDiscardsRemaining(discardsRemaining);
        UpdateAnte(currentAnte);
        UpdateRound(currentRound);

    }

    public void UpdateAnte(int ante)
    {
        currentAnte = ante;
        anteText.text = $"{currentAnte}";
    }

    public void UpdateRound(int round)
    {
        currentRound = round;
        roundText.text = $"{currentRound}";
    }


    public void UpdateLevelScore()
    {
        // Calculate the required score for the current level dynamically if needed
        if (currentRound == 1)
            scoreRequiredForLevel = 300;
        else if (currentRound == 2)
            scoreRequiredForLevel = 450;
        
        // Update the text
        scoreRequiredText.text = $"{scoreRequiredForLevel}";
    }

    // add the score from currently played hand to the score so far
    public void UpdatePlayerScore(int handFinalScore) //update the score in the end after evaluating the poker hand
    {
        pointsScored = handFinalScore;
        scoreCollectedText.text = $"{pointsScored}";
    }

    public void UpdateCurrentHandScore(int currentCardScore) // update the score in the blue box
    {
        handScore = currentCardScore;
        handScoreText.text = $"{handScore}";
    }

    public void UpdateMultiplier(float multiplier) // update the score in the red box
    {
        handMultiplier = multiplier;
        handMultiplierText.text = $"{handMultiplier}";
    }

    public void UpdatePokerHandText(string pokerHand)
    {
        pokerHandSelected = pokerHand;
        pokerHandSelectedText.text = $"{pokerHandSelected}";
    }

    public void DecrementDiscardsRemaining()
    {
        if (discardsRemaining > 0)
            discardsRemaining--;
        discardsRemainingText.text = $"{discardsRemaining}";

        resultManager.CheckGameState(); // needs to be done after scoring is over in CardPlayManager
    }

    public void SetDiscardsRemaining(int n)
    {
        discardsRemaining = n;
        discardsRemainingText.text = $"{discardsRemaining}";
    }

    public void DecrementHandsRemaining()
    {
        if (handsRemaining > 0)
            handsRemaining--;
        handsRemainingText.text = $"{handsRemaining}";

    }

    public void UpdateHandsRemaining(int n)
    {
        handsRemaining = n;
        handsRemainingText.text = $"{handsRemaining}";
    }

    public void NextLevel()
    {
        currentRound++;
        UpdateLevelScore();
        UpdatePlayerScore(0);
        handsRemaining = 5;
        discardsRemaining = 3;
        UpdateHandsRemaining(handsRemaining);
        SetDiscardsRemaining(discardsRemaining);
        UpdateAnte(currentAnte);
        UpdateRound(currentRound);
    }

    public void ResetGame()
    {
        currentRound = 1;
        currentAnte = 1;
        UpdateLevelScore();
        UpdatePlayerScore(0);
        UpdateHandsRemaining(handsRemaining);
        SetDiscardsRemaining(discardsRemaining);
        UpdateAnte(currentAnte);
        UpdateRound(currentRound);
    }
}

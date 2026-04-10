using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPlayManager : MonoBehaviour
{
    public Transform playArea; //where the cards will be played into
    public float maxSpacing = 2f; //maximum distance between cards
    private bool cardsOnTable = false;
    public List<Card> playedCards = new List<Card>();
    public PokerHandChecker handChecker;
    public HorizontalCardHolder handManager;
    public GameResultManager resultManager;

    [Header("Score Display")]
    public GameObject scoreTextPrefab;
    public Canvas gameCanvas;
    public float scoreDisplayOffsetY = 1.0f; // Offset Y above the card in world units
    public float delayBetweenScoreUpdates = 0.75f; // Time for each score popup
    public float scoreTextLifetime = 0.7f;

    void Start()
    {
        if (gameCanvas == null)
        {
            gameCanvas = FindFirstObjectByType<Canvas>();
            if (gameCanvas == null)
            {
                Debug.LogError("CardPlayManager: Game Canvas not found or assigned!");
            }
        }

        if (handManager == null)
        {
            handManager = FindFirstObjectByType<HorizontalCardHolder>();
            if (handManager == null)
                Debug.LogError("CardPlayManager: HandManager not found in scene!");
        }
        
        if (resultManager == null)
        {
            resultManager = FindFirstObjectByType<GameResultManager>();
        }

        if (scoreTextPrefab == null)
        {
            Debug.LogError("CardPlayManager: Score Text Prefab not assigned!");
        }
    }

    public void PlayCards() 
    {   
        List<Card> cardsToPlay = new List<Card>(Card.selectedCards);
        // play the selected cards
        if (cardsToPlay == null || cardsToPlay.Count == 0 || cardsOnTable)
            return;
        
        
        // Log selected cards for debugging with more detail
        Debug.Log($"Playing {cardsToPlay.Count} cards:");
        foreach (var card in cardsToPlay)
        {
            if (card.cardInfo != null)
            {
                Debug.Log($"Selected: {card.name} - {card.cardInfo.ToString()}");
                // Store reference to this card in a static way that can be accessed during discard
                if (!playedCardReferences.ContainsKey(card.name))
                {
                    playedCardReferences[card.name] = card;
                }
            }
            else
            {
                Debug.LogWarning($"Card {card.name} has no cardInfo component!");
            }
        }

        int count = cardsToPlay.Count;

        // Calculate spacing: shrink spacing if too many cards
        float spacing = Mathf.Min(maxSpacing, 6f / count);

        // Determine horizontal layout starting point
        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        List<Card> currentTurnPlayedCards = new List<Card>(); // Use a temporary list for this turn

        for (int i = 0; i < count; i++)
        {
            Card card = cardsToPlay[i];
            Vector3 targetPos = playArea.position + new Vector3(startX + i * spacing, 0, 0);
            // Optional: Animate card to position instead of teleporting
            card.transform.position = targetPos;
            currentTurnPlayedCards.Add(card); // Add to this turn's list

            if (card.cardInfo == null)
            {
                Debug.LogError($"Card {card.name} has no CardInfo component when being played!");
            }
            card.enabled = false; // Disable further selection
        }
        LevelManager.Instance.handsRemaining--;
        Card.selectedCards.Clear(); // Clear selection after cards are "moved"

        cardsOnTable = true; // Set flag that cards are now on table for this round
        playedCards = new List<Card>(currentTurnPlayedCards); // Update the main list for evaluation

        StartCoroutine(EvaluatePlayedHandCoroutine(this.playedCards)); // Start the coroutine
    }

    // Dictionary to keep track of played card references by name
    public static Dictionary<string, Card> playedCardReferences = new Dictionary<string, Card>();

    private IEnumerator EvaluatePlayedHandCoroutine(List<Card> cardsInPlay)
    {
        yield return new WaitForSeconds(1f);

        // Validate cards
        bool allCardsValid = true;
        foreach (Card card in cardsInPlay)
        {
            if (card.cardInfo == null)
            {
                Debug.LogError($"Card {card.name} missing CardInfo component during evaluation!");
                allCardsValid = false;
            }
        }
        if (!allCardsValid)
        {
            Debug.LogError("Cannot evaluate hand: one or more cards missing CardInfo. Aborting score sequence.");
            yield break;
        }

        var result = handChecker.EvaluateHand(cardsInPlay);
        List<Card> contributingCards = result.ContributingCards; // Cards that form the poker hand

        // --- Score Application Sequence ---
        int cumulativeRawScoreThisHand = 0; // Base score + sum of rank values of contributing cards

        // 1. Apply Base Score (if any, and if you want to show it separately)
        if (result.BaseScore > 0)
        {
            cumulativeRawScoreThisHand += result.BaseScore;
            Debug.Log($"Applied Base Score: {result.BaseScore}");
        }

        // 2. Apply score from each contributing card one-by-one
        Debug.Log($"Hand Type: {result.HandType}. Contributing cards for score display: {contributingCards.Count}");

        foreach (Card card in contributingCards)
        {
            CardInfo info = card.cardInfo;
            int scoreFromThisCard = info.rankValue; // This is the value to display

            // Instantiate and position the score text
            GameObject scoreTextGO = null;
            if (scoreTextPrefab != null && gameCanvas != null)
            {
                scoreTextGO = Instantiate(scoreTextPrefab, gameCanvas.transform);
                
                // --- Positioning the Text ---
                Vector3 worldPos = card.transform.position + Vector3.up * scoreDisplayOffsetY;
                Vector2 localPoint;

                RectTransform canvasRect = gameCanvas.GetComponent<RectTransform>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Camera.main.WorldToScreenPoint(worldPos), Camera.main, out localPoint);

                scoreTextGO.GetComponent<RectTransform>().anchoredPosition = localPoint;


                Text uiText = scoreTextGO.GetComponent<Text>();
                if (uiText != null) uiText.text = $"+{scoreFromThisCard}";
                
                Destroy(scoreTextGO, scoreTextLifetime);
            }

            // Update LevelManager's score with this card's individual contribution
            cumulativeRawScoreThisHand += scoreFromThisCard;
            LevelManager.Instance.UpdateCurrentHandScore(cumulativeRawScoreThisHand);
            LevelManager.Instance.awaitingFinalScoreEvaluation = true;

            Debug.Log($"Card: {info.cardName}, Displaying Score: +{scoreFromThisCard}, Added to Player Score. Cumulative Raw: {cumulativeRawScoreThisHand}");


            yield return new WaitForSeconds(delayBetweenScoreUpdates);
        }

        // 3. Apply the Multiplier
        // The formula is: FinalScore = (BaseScore + SumOfContributingCardRankValues) * Multiplier
        // We've already added BaseScore and SumOfContributingCardRankValues to the player's score.
        // So, we need to add the *additional* points gained from the multiplier.
        // All these need to be added onto the score payer already has accumulated this round
        int finalScoreForHand = Mathf.RoundToInt(cumulativeRawScoreThisHand * result.Multiplier);
        int currentScore = LevelManager.Instance.pointsScored;
        LevelManager.Instance.UpdatePlayerScore(currentScore + finalScoreForHand);

        // Short delay before cards can be discarded or next play
        yield return new WaitForSeconds(0.5f);
        // cardsOnTable = false; // Reset for next play - or do this when cards are discarded.
                                 // Keeping it true until Discard might be better.

        // At this point, players can choose to discard or the game moves on.
        // If you want to automatically discard:
        DiscardCardsFromTable(cardsInPlay);
    }


    public void DiscardCardsButton()
    {
        if (Card.selectedCards.Count == 0 || LevelManager.Instance.discardsRemaining <= 0) 
            return;

        // Create a copy of the list to avoid modification during iteration
        List<Card> cardsToDiscard = new List<Card>(Card.selectedCards);
        List<string> deletedCardNames = new List<string>();
        
        foreach (Card card in cardsToDiscard)
        {
            // Keep track of which cards being discarded
            deletedCardNames.Add(card.name);
            
            // Animate and destroy the played card
            StartCoroutine(FlyOffAndDestroy(card.gameObject));
        }
        
        // Tell the handManager to process discard directly
        if (handManager != null)
        {
            // Pass the names of discarded cards
            StartCoroutine(ProcessDiscardAndRefill(deletedCardNames));
        }
        else
        {
            Debug.LogError("HandManager reference is null. Cannot process discard properly.");
        }


        playedCards.Clear();
        Card.selectedCards.Clear();
        cardsOnTable = false;
        
        LevelManager.Instance.UpdateMultiplier(0);
        LevelManager.Instance.UpdateCurrentHandScore(0);
        LevelManager.Instance.UpdatePlayerScore(0);
        
        LevelManager.Instance.DecrementDiscardsRemaining();
    }

    public void DiscardCardsFromTable(List<Card> playedCards) 
    {
        // same visual functionality, just also decrements discards that the player has left.
        if (playedCards.Count == 0) return;

        // Create a copy of the list to avoid modification during iteration
        List<Card> cardsToDiscard = new List<Card>(playedCards);
        
        Debug.Log("DiscardCardsFromTable - Cards in handManager: " + (handManager != null ? handManager.cards.Count.ToString() : "handManager is null"));
        
        foreach (Card card in cardsToDiscard)
        {
            Debug.Log($"Trying to discard: {card.name}");
        }
        
        // Debug handManager cards list to see what cards are available
        if (handManager != null && handManager.cards != null)
        {
            Debug.Log("HandManager cards:");
            foreach (Card card in handManager.cards)
            {
                Debug.Log($"  Hand contains: {card.name}");
            }
        }
        
        // Instead of trying to find the card in the handManager.cards list,
        // we'll directly tell the handManager to delete and refill
        List<string> deletedCardNames = new List<string>();
        
        foreach (Card card in cardsToDiscard)
        {
            // Keep track of which cards are being discarded
            deletedCardNames.Add(card.name);
            
            // Animate and destroy the played card
            StartCoroutine(FlyOffAndDestroy(card.gameObject));
        }
        
        // IMPORTANT: Instead of trying to match cards, tell the handManager to process discard directly
        if (handManager != null)
        {
            // Pass the names of discarded cards
            StartCoroutine(ProcessDiscardAndRefill(deletedCardNames));
        }
        else
        {
            Debug.LogError("HandManager reference is null. Cannot process discard properly.");
        }

        playedCards.Clear();
        cardsOnTable = false;

        LevelManager.Instance.DecrementDiscardsRemaining();
        LevelManager.Instance.DecrementHandsRemaining();
    }
    
    private IEnumerator ProcessDiscardAndRefill(List<string> cardNames)
    {
        // Wait for animations to start
        yield return new WaitForSeconds(0.2f);
        
        if (handManager != null)
        {
            // First, manually clean up any slots with empty cards (defensive)
            handManager.CleanupEmptySlots();
            
            // Now tell the manager to delete the cards by name and refill
            handManager.DeleteCardsByName(cardNames);
            
            // Allow the handManager to handle the refill
            Debug.Log("ProcessDiscardAndRefill: Telling handManager to refill the hand");
            handManager.RefillHand();
        }
    }
    
    private IEnumerator DelayedRefillHand()
    {
        // Short delay to ensure all card destruction processes have started
        yield return new WaitForSeconds(0.1f);
        
        if (handManager != null)
        {
            Debug.Log("Refilling hand with new cards...");
            handManager.RefillHand();
        }
        else
        {
            Debug.LogError("Cannot refill hand: handManager reference is null!");
        }
    }
    
    private IEnumerator FlyOffAndDestroy(GameObject cardGO)
    {
        if (cardGO == null) yield break;

        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 startPos = cardGO.transform.position;
        Vector3 screenCenterRight = Camera.main.ScreenToWorldPoint(
            new Vector3(Screen.width + 200f, Screen.height / 2f, Camera.main.WorldToScreenPoint(startPos).z)
        );

        while (elapsed < duration && cardGO != null) // Check cardGO in loop
        {
            cardGO.transform.position = Vector3.Lerp(startPos, screenCenterRight, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (cardGO != null) // Final check before destroy
        {
            Destroy(cardGO);
        }
    }
    
    private IEnumerator FlyOffAndDestroyWithSlot(GameObject cardGO, GameObject slotGO)
    {
        if (cardGO == null) yield break;

        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 startPos = cardGO.transform.position;
        Vector3 screenCenterRight = Camera.main.ScreenToWorldPoint(
            new Vector3(Screen.width + 200f, Screen.height / 2f, Camera.main.WorldToScreenPoint(startPos).z)
        );

        while (elapsed < duration && cardGO != null) // Check cardGO in loop
        {
            cardGO.transform.position = Vector3.Lerp(startPos, screenCenterRight, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destroy both the card and its slot container
        if (cardGO != null)
        {
            Destroy(cardGO);
        }
        
        if (slotGO != null)
        {
            Destroy(slotGO);
        }
    }
}

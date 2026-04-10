using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

    

public class PokerHandChecker : MonoBehaviour
{
    [HideInInspector] public int straightLength = 5;
    // there's a joker that allows to be made with 4

    public PokerHandUIController uiController;

    public enum PokerHandType
    {
        High_Card,
        Pair,
        Two_Pair,
        Three_of_a_Kind,
        Straight,
        Flush,
        Full_House,
        Four_of_a_Kind,
        Straight_Flush
    }
    
    [System.Serializable]
    public class HandInfo
    {
        public PokerHandType handType;
        public int baseScore;
        public float multiplier;

        // Will store the base and multipliers of each hand
        public HandInfo(PokerHandType type, int score, float mult)
        {
            handType = type;
            baseScore = score;
            multiplier = mult;
        }
    }

    // the result will be returned as an object of the following class
    public class HandEvaluationResult
    {
        public PokerHandType HandType;
        public int BaseScore;
        public float Multiplier;
        public List<Card> ContributingCards; // Stores the cards that form the hand

        public HandEvaluationResult(PokerHandType handType, int baseScore, float multiplier, List<Card> contributingCards)
        {
            HandType = handType;
            BaseScore = baseScore;
            Multiplier = multiplier;
            ContributingCards = contributingCards ?? new List<Card>();
        }
    }

    // The base score and multipliers of each hand:
    private Dictionary<PokerHandType, HandInfo> handScoreTable = new Dictionary<PokerHandType, HandInfo>()
    {
        { PokerHandType.High_Card,      new HandInfo(PokerHandType.High_Card, 5, 1f) },
        { PokerHandType.Pair,       new HandInfo(PokerHandType.Pair, 10, 2f) },
        { PokerHandType.Two_Pair,       new HandInfo(PokerHandType.Two_Pair, 20, 2f) },
        { PokerHandType.Three_of_a_Kind,  new HandInfo(PokerHandType.Three_of_a_Kind, 30, 3f) },
        { PokerHandType.Straight,      new HandInfo(PokerHandType.Straight, 30, 4f) },
        { PokerHandType.Flush,         new HandInfo(PokerHandType.Flush, 35, 4f) },
        { PokerHandType.Full_House,     new HandInfo(PokerHandType.Full_House, 40, 4f) },
        { PokerHandType.Four_of_a_Kind,   new HandInfo(PokerHandType.Four_of_a_Kind, 60, 7f) },
        { PokerHandType.Straight_Flush, new HandInfo(PokerHandType.Straight_Flush, 100, 8f) },
    };

    // main function
    public HandEvaluationResult EvaluateHand(List<Card> playedCards)
    {
        PokerHandType detectedHand = PokerHandType.High_Card; //default to high card
        
        List<Card> cards = new List<Card>(playedCards);
        cards = InsertionSort(cards); // sort the copy
        List<Card> contributingHandCards = new List<Card>();

        // Check for hands in decreasing order of strength
        List<Card> foundCards;
        List<Card> sortedCards = InsertionSort(new List<Card>(playedCards));
        
        if ((foundCards = GetStraightFlushCards(sortedCards)) != null)
        {
            detectedHand = PokerHandType.Straight_Flush;
            contributingHandCards = foundCards;
        }
        else if ((foundCards = GetFourOfAKindCards(sortedCards)) != null)
        {
            detectedHand = PokerHandType.Four_of_a_Kind;
            contributingHandCards = foundCards;
        }
        else if ((foundCards = GetFullHouseCards(sortedCards)) != null)
        {
            detectedHand = PokerHandType.Full_House;
            contributingHandCards = foundCards;
        }
        else if ((foundCards = GetFlushCards(sortedCards)) != null) // Assumes flush needs `straightLength` cards
        {
            detectedHand = PokerHandType.Flush;
            contributingHandCards = foundCards;
        }
        else if ((foundCards = GetStraightCards(sortedCards)) != null)
        {
            detectedHand = PokerHandType.Straight;
            contributingHandCards = foundCards;
        }
        else if ((foundCards = GetThreeOfAKindCards(sortedCards)) != null)
        {
            detectedHand = PokerHandType.Three_of_a_Kind;
            contributingHandCards = foundCards;
        }
        else if ((foundCards = GetTwoPairCards(sortedCards)) != null)
        {
            detectedHand = PokerHandType.Two_Pair;
            contributingHandCards = foundCards;
        }
        else if ((foundCards = GetPairCards(sortedCards)) != null)
        {
            detectedHand = PokerHandType.Pair;
            contributingHandCards = foundCards;
        }
        else // High Card
        {
            detectedHand = PokerHandType.High_Card;
            // For High Card, we only consider the single highest card as "contributing"
            Card highestCard = sortedCards[sortedCards.Count - 1];
            contributingHandCards = new List<Card> { highestCard };
        }
        
        var handDetails = handScoreTable[detectedHand];
        LevelManager.Instance.UpdatePokerHandText(detectedHand.ToString().Replace('_', ' '));
        LevelManager.Instance.UpdateCurrentHandScore(handDetails.baseScore);
        LevelManager.Instance.UpdateMultiplier(handDetails.multiplier);

        // Log contributing cards for debugging
        // Debug.Log($"Detected Hand: {detectedHand}. Contributing Cards ({contributingHandCards.Count}):");
        // foreach(var card in contributingHandCards)
        // {
        //     Debug.Log($"- {card.cardInfo.ToString()}");
        // }

        if (uiController != null)
        {
            uiController.AddNewRecord(handDetails.baseScore, handDetails.multiplier, detectedHand.ToString().Replace('_', ' '));
            Debug.Log("��֪ͨUI���������¼�¼");
        }
        else
        {
            Debug.LogWarning("PokerHandUIController����δ���ã�");
        }

        return new HandEvaluationResult(detectedHand, handDetails.baseScore, handDetails.multiplier, contributingHandCards);
    }

    // --- Hand Evaluation Helper Methods ---
    // These methods now return List<Card> if the hand is found, otherwise null.

    private List<Card> GetStraightFlushCards(List<Card> sortedCards)
    {
        if (sortedCards.Count < straightLength) return null;

        // Check for Ace-low straight flush (A-2-3-4-5 of the same suit) - "Steel Wheel"
        // Assumes straightLength == 5 for wheel.
        if (straightLength == 5 && sortedCards.Count >= 5)
        {
            bool isAceLowStructure = sortedCards[0].cardInfo.secondarySortValue == 2 && // 2
                                     sortedCards[1].cardInfo.secondarySortValue == 3 && // 3
                                     sortedCards[2].cardInfo.secondarySortValue == 4 && // 4
                                     sortedCards[3].cardInfo.secondarySortValue == 5 && // 5
                                     sortedCards[4].cardInfo.secondarySortValue == 14;  // Ace (high value)
            if (isAceLowStructure)
            {
                string suit = sortedCards[0].cardInfo.suitName;
                bool sameSuit = true;
                for (int k = 1; k < 5; k++)
                {
                    if (sortedCards[k].cardInfo.suitName != suit)
                    {
                        sameSuit = false;
                        break;
                    }
                }
                if (sameSuit)
                {
                    return new List<Card> { sortedCards[0], sortedCards[1], sortedCards[2], sortedCards[3], sortedCards[4] };
                }
            }
        }

        // Check for general straight flushes (iterate backwards to find highest)
        for (int i = sortedCards.Count - 1; i >= straightLength - 1; i--)
        {
            // Potential straight: sortedCards[i-straightLength+1] to sortedCards[i]
            bool isStraightSequence = true;
            for (int j = 0; j < straightLength - 1; j++) // Check consecutiveness
            {
                if (sortedCards[i - j].cardInfo.secondarySortValue - sortedCards[i - j - 1].cardInfo.secondarySortValue != 1)
                {
                    isStraightSequence = false;
                    break;
                }
            }

            if (isStraightSequence)
            {
                string suit = sortedCards[i - straightLength + 1].cardInfo.suitName;
                bool isFlushSequence = true;
                for (int k = 1; k < straightLength; k++) // Check same suit
                {
                    if (sortedCards[i - straightLength + 1 + k].cardInfo.suitName != suit)
                    {
                        isFlushSequence = false;
                        break;
                    }
                }
                if (isFlushSequence)
                {
                    List<Card> sfHand = new List<Card>();
                    for (int k = 0; k < straightLength; k++)
                    {
                        sfHand.Add(sortedCards[i - straightLength + 1 + k]);
                    }
                    return sfHand;
                }
            }
        }
        return null;
    }

    private List<Card> GetFourOfAKindCards(List<Card> sortedCards)
    {
        if (sortedCards.Count < 4) return null;
        for (int i = 0; i <= sortedCards.Count - 4; i++)
        {
            if (sortedCards[i].cardInfo.secondarySortValue == sortedCards[i + 1].cardInfo.secondarySortValue &&
                sortedCards[i].cardInfo.secondarySortValue == sortedCards[i + 2].cardInfo.secondarySortValue &&
                sortedCards[i].cardInfo.secondarySortValue == sortedCards[i + 3].cardInfo.secondarySortValue)
            {
                return new List<Card> { sortedCards[i], sortedCards[i + 1], sortedCards[i + 2], sortedCards[i + 3] };
            }
        }
        return null;
    }

    private List<Card> GetFullHouseCards(List<Card> sortedCards)
    {
        if (sortedCards.Count < 5) return null;

        Dictionary<int, List<Card>> rankMap = GroupCardsByRank(sortedCards);

        List<Card> trips = null;
        int tripsRankVal = -1;
        List<Card> pair = null;

        // Find the highest ranking three-of-a-kind
        foreach (var rankVal in rankMap.Keys.OrderByDescending(r => r))
        {
            if (rankMap[rankVal].Count >= 3)
            {
                trips = rankMap[rankVal].GetRange(0, 3);
                tripsRankVal = rankVal;
                break;
            }
        }

        if (trips == null) return null;

        // Find the highest ranking pair from a *different* rank
        foreach (var rankVal in rankMap.Keys.OrderByDescending(r => r))
        {
            if (rankVal != tripsRankVal && rankMap[rankVal].Count >= 2)
            {
                pair = rankMap[rankVal].GetRange(0, 2);
                break;
            }
        }

        if (pair != null)
        {
            List<Card> fullHouseHand = new List<Card>(trips);
            fullHouseHand.AddRange(pair);
            return fullHouseHand;
        }
        return null;
    }

    private List<Card> GetFlushCards(List<Card> sortedCards)
    {
        // A flush requires at least `straightLength` cards (typically 5) of the same suit.
        if (sortedCards.Count < straightLength) return null;

        string suit = sortedCards[0].cardInfo.suitName;
        for (int i = 1; i < sortedCards.Count; i++)
        {
            if (sortedCards[i].cardInfo.suitName != suit)
            {
                return null; // Not all cards are the same suit
            }
        }
        // If all cards passed (which must be >= straightLength) are the same suit, it's a flush
        // Return all cards that form this flush (which is all of them in this context if count matches hand size)
        // If more than `straightLength` cards are played and all are same suit, it's still a flush with all of them.
        // For poker, usually you take the best 5 if more are available.
        // Here, `sortedCards` is the hand. So if all are same suit, they all make the flush.
        return new List<Card>(sortedCards.GetRange(sortedCards.Count - straightLength, straightLength)); // Return highest `straightLength` cards
        // Or, if any 5 cards make a flush, return those. This is simpler:
        // return new List<Card>(sortedCards); // If all cards in the hand are the same suit.
    }


    private List<Card> GetStraightCards(List<Card> sortedCards)
    {
        if (sortedCards.Count < straightLength) return null;

        // Check for Ace-low straight (A-2-3-4-5) - "Wheel"
        // Assumes straightLength == 5 for wheel.
        if (straightLength == 5 && sortedCards.Count >= 5)
        {
            bool isAceLowStructure = sortedCards[0].cardInfo.secondarySortValue == 2 && // 2
                                     sortedCards[1].cardInfo.secondarySortValue == 3 && // 3
                                     sortedCards[2].cardInfo.secondarySortValue == 4 && // 4
                                     sortedCards[3].cardInfo.secondarySortValue == 5 && // 5
                                     sortedCards[4].cardInfo.secondarySortValue == 14;  // Ace (high value)
            if (isAceLowStructure)
            {
                // Check if these cards are NOT all the same suit (already handled by StraightFlush check)
                // This check is not strictly needed here if StraightFlush is checked first.
                return new List<Card> { sortedCards[0], sortedCards[1], sortedCards[2], sortedCards[3], sortedCards[4] };
            }
        }

        // Check for general straights (iterate backwards to find highest)
        for (int i = sortedCards.Count - 1; i >= straightLength - 1; i--)
        {
            // Potential straight: sortedCards[i-straightLength+1] to sortedCards[i]
            bool isStraightSequence = true;
            for (int j = 0; j < straightLength - 1; j++)
            {
                if (sortedCards[i - j].cardInfo.secondarySortValue - sortedCards[i - j - 1].cardInfo.secondarySortValue != 1)
                {
                    isStraightSequence = false;
                    break;
                }
            }
            if (isStraightSequence)
            {
                // Optional: check if it's NOT a flush (already handled by StraightFlush check)
                List<Card> straightHand = new List<Card>();
                for (int k = 0; k < straightLength; k++)
                {
                    straightHand.Add(sortedCards[i - straightLength + 1 + k]);
                }
                return straightHand;
            }
        }
        return null;
    }

    private List<Card> GetThreeOfAKindCards(List<Card> sortedCards)
    {
        if (sortedCards.Count < 3) return null;
        // Iterate backwards to find the highest three of a kind first
        for (int i = sortedCards.Count - 1; i >= 2; i--) // i is the highest index of the potential three
        {
            if (sortedCards[i].cardInfo.secondarySortValue == sortedCards[i - 1].cardInfo.secondarySortValue &&
                sortedCards[i].cardInfo.secondarySortValue == sortedCards[i - 2].cardInfo.secondarySortValue)
            {
                return new List<Card> { sortedCards[i - 2], sortedCards[i - 1], sortedCards[i] };
            }
        }
        return null;
    }

    private List<Card> GetTwoPairCards(List<Card> sortedCards)
    {
        if (sortedCards.Count < 4) return null;

        Dictionary<int, List<Card>> rankMap = GroupCardsByRank(sortedCards);
        List<List<Card>> pairs = new List<List<Card>>();

        foreach (var rankVal in rankMap.Keys.OrderByDescending(r => r)) // Process highest ranks first
        {
            if (rankMap[rankVal].Count >= 2)
            {
                pairs.Add(rankMap[rankVal].GetRange(0, 2)); // Add the pair
            }
        }

        if (pairs.Count >= 2)
        {
            // We have at least two pairs, `pairs` list is already sorted by rank (descending) due to OrderByDescending
            List<Card> twoPairHand = new List<Card>();
            twoPairHand.AddRange(pairs[0]); // Highest pair
            twoPairHand.AddRange(pairs[1]); // Second highest pair
            return twoPairHand;
        }
        return null;
    }

    private List<Card> GetPairCards(List<Card> sortedCards)
    {
        if (sortedCards.Count < 2) return null;
        // Iterate backwards to find the highest pair first
        for (int i = sortedCards.Count - 1; i > 0; i--)
        {
            if (sortedCards[i].cardInfo.secondarySortValue == sortedCards[i - 1].cardInfo.secondarySortValue)
            {
                return new List<Card> { sortedCards[i - 1], sortedCards[i] };
            }
        }
        return null;
    }

    // --- Utility Helper Methods ---

    private Dictionary<int, List<Card>> GroupCardsByRank(List<Card> cards)
    {
        Dictionary<int, List<Card>> rankMap = new Dictionary<int, List<Card>>();
        foreach (Card card in cards)
        {
            int rankVal = card.cardInfo.secondarySortValue;
            if (!rankMap.ContainsKey(rankVal))
            {
                rankMap[rankVal] = new List<Card>();
            }
            rankMap[rankVal].Add(card);
        }
        return rankMap;
    }

// Helper methods:
    private List<Card> InsertionSort(List<Card> cards) 
    {
        List<Card> cardsCopy = new List<Card>(cards);
        int n = cardsCopy.Count;
        for (int i = 1; i < n; i++) 
        {
            int key = cardsCopy[i].cardInfo.secondarySortValue;
            Card keyCard = cardsCopy[i];
            int j = i-1;
            while (j >= 0 && cardsCopy[j].cardInfo.secondarySortValue > key) 
            {
                cardsCopy[j+1] = cardsCopy[j];
                j--;
            }
            cardsCopy[j+1] = keyCard;
        }
        return cardsCopy;
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class CardOrganizer : MonoBehaviour
{
    // Reference to the Deck GameObject in the Hierarchy
    public Transform deckRoot;

    // Map rank letters to numeric values for sorting (higher is stronger)
    [HideInInspector] public Dictionary<char, int> rankMap = new Dictionary<char, int>
    {
        { 'A', 11 }, // Ace
        { 'M', 10 }, // King
        { 'L', 10 }, // Queen
        { 'J', 10 }, // Jack
        { 'K', 10 }, // Ten
        { 'I', 9 },  // Nine
        { 'H', 8 },  // Eight
        { 'G', 7 },  // Seven
        { 'F', 6 },  // Six
        { 'E', 5 },  // Five
        { 'D', 4 },  // Four
        { 'C', 3 },  // Three
        { 'B', 2 },  // Two
    };

    private Dictionary<char, int> tieBreakerRankMap = new Dictionary<char, int>
    {
        { 'A', 14 }, // Ace highest for tie-breaking
        { 'M', 13 }, // King
        { 'L', 12 }, // Queen
        { 'J', 11 }, // Jack
        { 'K', 10 }, // Ten
        { 'I', 9 },
        { 'H', 8 },
        { 'G', 7 },
        { 'F', 6 },
        { 'E', 5 },
        { 'D', 4 },
        { 'C', 3 },
        { 'B', 2 }
    };

    // Suit categories
    private enum Suit { Spade, Hearts, Diamonds, Clubs }

    // Sorted cards grouped by suit
    private Dictionary<Suit, List<GameObject>> sortedCards = new Dictionary<Suit, List<GameObject>>();

    void Start()
    {
        CategorizeAndSortCards();
    }

    void CategorizeAndSortCards()
    {
        // Initialize suit groups
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            sortedCards[suit] = new List<GameObject>();
        }

        // Loop through each child under Deck
        foreach (Transform card in deckRoot)
        {
            string name = card.name.ToLower();

            if (string.IsNullOrEmpty(name) || name.Length < 2) continue;

            char rankChar = char.ToUpper(card.name[0]);
            if (!rankMap.ContainsKey(rankChar) || !tieBreakerRankMap.ContainsKey(rankChar)) 
            {
                Debug.LogWarning($"Card: {card.name}, RankChar: {rankChar}. Key missing in rankMap or tieBreakerRankMap. Skipping CardInfo update for this card.");
                continue;
            }

            // Categorize by suit based on name
            Suit? suit = null;

            if (name.Contains("spade"))
                suit = Suit.Spade;
            else if (name.Contains("hearts"))
                suit = Suit.Hearts;
            else if (name.Contains("diamonds"))
                suit = Suit.Diamonds;
            else if (name.Contains("clubs"))
                suit = Suit.Clubs;
            
            if (suit != null) 
            {
                sortedCards[suit.Value].Add(card.gameObject);

                // The children of deckRoot are source/template cards.
                // We just want to ensure they have CardInfo populated.
                // They don't necessarily need the full 'Card.cs' script.
                var sourceCardInfo = card.GetComponent<CardInfo>();
                if (sourceCardInfo == null)
                {
                    Debug.Log($"CardInfo component missing on source card {card.name}, adding it.");
                    sourceCardInfo = card.gameObject.AddComponent<CardInfo>();
                }
                sourceCardInfo.cardName = card.name; // e.g., "ASpade"
                sourceCardInfo.rankChar = rankChar;
                sourceCardInfo.rankValue = rankMap[rankChar];
                sourceCardInfo.secondarySortValue = tieBreakerRankMap[rankChar]; // NEW
                sourceCardInfo.suitName = suit.ToString(); // e.g., "Spade"

                if (sourceCardInfo.HasValidInfo())
                {
                    // Debug.Log($"Successfully set/updated CardInfo on source card: {sourceCardInfo.ToString()}");
                }
                else
                {
                    Debug.LogError($"Failed to set valid CardInfo for source card {card.name}");
                }
                }
        }

        // Sort cards within each suit group by rank
        foreach (var kvp in sortedCards)
        {
            kvp.Value.Sort((a, b) =>
            {
                char aRank = char.ToUpper(a.name[0]);
                char bRank = char.ToUpper(b.name[0]);
                return rankMap[bRank].CompareTo(rankMap[aRank]); // Descending
            });
        }

        // Output sorted result to console
        // ���ÿ�黨ɫ����������
        foreach (var suit in sortedCards)
        {
            Debug.Log($"--- {suit.Key} ---");
            foreach (var card in suit.Value)
            {
                char rankChar = char.ToUpper(card.name[0]);
                int rank = rankMap.ContainsKey(rankChar) ? rankMap[rankChar] : -1;
                Debug.Log($"{card.name} (rank: {rank})");
            }
        }
    }
}
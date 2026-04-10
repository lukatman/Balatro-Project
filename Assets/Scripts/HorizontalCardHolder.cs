using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HorizontalCardHolder : MonoBehaviour
{
    [SerializeField] private Card selectedCard;
    [SerializeField] private Card hoveredCard;

    [SerializeField] private GameObject slotPrefab;     // Container for each card
    [SerializeField] private GameObject cardPrefab;     // Card prefab to instantiate
    private RectTransform rect;

    [Header("Spawn Settings")]
    [SerializeField] public int handSize = 8;           // Number of cards in hand
    public List<Card> cards;                            // Currently held cards

    private List<GameObject> remainingDeck = new List<GameObject>(); // Cards left in the deck

    bool isCrossing = false;                            // Prevents multiple swaps at the same time
    [SerializeField] private bool tweenCardReturn = true; // Smooth return animation toggle

    [Header("Audio Settings")]
    public AudioClip cardClickSound;                    // Sound when selecting a card
    private AudioSource audioSource;

    // Find cards from the "Deck" GameObject that are valid and have CardInfo
    private List<GameObject> GetSourceCardObjects()
    {
        List<GameObject> result = new List<GameObject>();
        GameObject deckObj = GameObject.Find("Deck");

        if (deckObj == null)
        {
            Debug.LogError("Deck object not found.");
            return result;
        }

        foreach (Transform child in deckObj.transform)
        {
            string name = child.name.ToLower();
            bool isValidSuit = name.Contains("spade") || name.Contains("hearts") || name.Contains("diamonds") || name.Contains("clubs");
            bool isValidRank = name.Length > 0 && char.IsLetter(name[0]);

            if (isValidSuit && isValidRank)
            {
                if (child.GetComponent<CardInfo>() != null && child.GetComponent<CardInfo>().HasValidInfo())
                {
                    result.Add(child.gameObject);
                }
            }
        }

        return result;
    }

    void Start()
    {
        rect = GetComponent<RectTransform>();
        cards = new List<Card>();

        // Initialize audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Initialize and shuffle remaining deck
        remainingDeck = GetSourceCardObjects().OrderBy(x => UnityEngine.Random.value).ToList();

        DealInitialHand(); // Deal cards at game start
    }

    // Deal the initial hand at game start
    private void DealInitialHand()
    {
        int cardsToDeal = Mathf.Min(handSize, remainingDeck.Count);

        for (int i = 0; i < cardsToDeal; i++)
        {
            AddCardFromDeck();
        }

        StartCoroutine(UpdateVisualIndexes());
    }

    // Add a single card from the remaining deck to the hand
    private void AddCardFromDeck()
    {
        if (remainingDeck.Count == 0)
        {
            Debug.Log("Deck is empty. No more cards to draw.");
            return;
        }

        GameObject sourceCard = remainingDeck[0];
        remainingDeck.RemoveAt(0);

        CardInfo sourceCardInfo = sourceCard.GetComponent<CardInfo>();
        Sprite sourceSprite = sourceCard.GetComponent<Image>()?.sprite;

        if (sourceCardInfo == null || sourceSprite == null)
        {
            Debug.LogError("Invalid card in deck. Skipping.");
            return;
        }

        GameObject slot = Instantiate(slotPrefab, transform);
        GameObject cardObj = Instantiate(cardPrefab, slot.transform);

        Card card = cardObj.GetComponent<Card>();
        if (card == null)
        {
            Debug.LogError("Card prefab missing Card script.");
            Destroy(cardObj);
            Destroy(slot);
            return;
        }

        // Initialize the new card with sprite and info
        card.Initialize(sourceSprite, sourceCardInfo);
        cardObj.name = $"HandCard_{sourceCardInfo.cardName}";

        cards.Add(card);

        // Bind interaction events
        card.PointerEnterEvent.AddListener(CardPointerEnter);
        card.PointerExitEvent.AddListener(CardPointerExit);
        card.BeginDragEvent.AddListener(BeginDrag);
        card.EndDragEvent.AddListener(EndDrag);
        card.SelectEvent.AddListener(OnCardSelect);
    }

    // Fill hand back to desired size after deletion
    public void RefillHand()
    {   
        // Debug the state before modifications
        DebugHandState();
        
        // Clean up null references in the cards list
        cards.RemoveAll(card => card == null);
        
        // Log current state for debugging
        Debug.Log($"RefillHand called. Current cards: {cards.Count}, Target: {handSize}");
        
        // Calculate how many cards need to be added using the fixed target size
        int cardsToAdd = handSize - cards.Count;
        Debug.Log($"Adding {cardsToAdd} cards to hand");
        
        // Add new cards until we reach the target hand size
        for (int i = 0; i < cardsToAdd; i++)
        {
            if (remainingDeck.Count > 0)
            {
                AddCardFromDeck();
            }
            else
            {
                Debug.LogWarning("No more cards in deck to draw!");
                break;
            }
        }

        // Update visual positions of all cards
        StartCoroutine(UpdateVisualIndexes());
        
        // Debug again to verify changes
        Debug.Log("After refill:");
        DebugHandState();
    }

    // The fix needs to ensure DeleteCardsByName works properly too
    public void DeleteCardsByName(List<string> cardNames)
    {
        if (cardNames == null || cardNames.Count == 0)
        {
            Debug.LogWarning("DeleteCardsByName called with empty list");
            return;
        }
        
        Debug.Log($"DeleteCardsByName: Trying to delete {cardNames.Count} cards");
        
        // Store which slots need to be destroyed
        List<GameObject> slotsToDestroy = new List<GameObject>();
        
        // Track which cards were actually removed
        int cardsRemoved = 0;
        
        foreach (string cardName in cardNames)
        {
            bool found = false;
            
            // First try exact match
            foreach (Card card in cards.ToArray()) // Use ToArray to avoid collection modification issues
            {
                if (card != null && card.name == cardName)
                {
                    Debug.Log($"Deleting card {cardName} from hand");
                    found = true;
                    
                    // Store reference to parent slot GameObject
                    GameObject slotObject = card.transform.parent.gameObject;
                    slotsToDestroy.Add(slotObject);
                    
                    // Remove from cards list
                    cards.Remove(card);
                    cardsRemoved++;
                    
                    // Destroy the card (the slot will be destroyed later)
                    Destroy(card.gameObject);
                    break;
                }
            }
            
            // If not found, try a fuzzy search
            if (!found)
            {
                foreach (Card card in cards.ToArray())
                {
                    if (card != null && card.name.Contains(cardName.Split('_').LastOrDefault() ?? ""))
                    {
                        Debug.Log($"Found similar card {card.name} for {cardName}, deleting");
                        
                        // Store reference to parent slot GameObject
                        GameObject slotObject = card.transform.parent.gameObject;
                        slotsToDestroy.Add(slotObject);
                        
                        // Remove from cards list
                        cards.Remove(card);
                        cardsRemoved++;
                        
                        // Destroy the card (the slot will be destroyed later)
                        Destroy(card.gameObject);
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    Debug.LogWarning($"Card {cardName} not found in hand for deletion");
                }
            }
        }
        
        // Destroy all the slot objects after removing all cards
        foreach (GameObject slot in slotsToDestroy)
        {
            Destroy(slot);
        }

        FindFirstObjectByType<GameResultManager>()?.CheckGameState();

        // Log the remaining cards
        Debug.Log($"After deletion: {cards.Count} cards remaining in hand. Removed {cardsRemoved} cards.");
    }

    public void CleanupEmptySlots()
    {
        List<Transform> emptySlots = new List<Transform>();
        
        // Find all empty slots
        foreach (Transform child in transform)
        {
            if (child.childCount == 0)
            {
                emptySlots.Add(child);
            }
        }
        
        // Delete empty slots
        foreach (Transform slot in emptySlots)
        {
            Debug.Log($"Destroying empty slot {slot.name}");
            Destroy(slot.gameObject);
        }
        
        Debug.Log($"CleanupEmptySlots: Removed {emptySlots.Count} empty slots");
    }
    
    public void DeleteCard(Card cardToDelete)
    {
        if (cardToDelete == null)
        {
            Debug.LogWarning("DeleteCard called with null reference.");
            return;
        }

        if (cards.Contains(cardToDelete))
        {
            cards.Remove(cardToDelete);
            Destroy(cardToDelete.gameObject); // Remove from scene and memory
            Debug.Log($"Card '{cardToDelete.name}' deleted successfully.");
        }
        else
        {
            Debug.LogWarning($"Attempted to delete a card not in hand: {cardToDelete.name}");
        }
    }

    // Update visual indexes after adding/removing cards
    private IEnumerator UpdateVisualIndexes()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        foreach (Card card in cards)
        {
            card.cardVisual?.UpdateIndex(transform.childCount);
        }
    }

    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }

    private void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        // Return card to position if not dropped
        selectedCard.transform.DOLocalMove(
            selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero,
            tweenCardReturn ? 0.15f : 0
        ).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;
    }

    private void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    private void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }

    void Update()
    {
        // Delete hovered card with Delete key
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject); // Destroy slot + card
                cards.Remove(hoveredCard);                        // Remove from hand list
                RefillHand();                                     // Add new card from deck
                FindFirstObjectByType<GameResultManager>()?.CheckGameState();
            }
        }

        // Right mouse button deselects all cards
        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }

        if (selectedCard == null || isCrossing)
            return;

        // Handle drag-based card swapping
        for (int i = 0; i < cards.Count; i++)
        {
            if (selectedCard.transform.position.x > cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    // Swap card with another in the hand
    private void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected
            ? new Vector3(0, cards[index].selectionOffset, 0)
            : Vector3.zero;

        selectedCard.transform.SetParent(crossedParent);
        isCrossing = false;

        if (cards[index].cardVisual == null) return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        // Recalculate visual positions
        foreach (Card card in cards)
        {
            card.cardVisual?.UpdateIndex(transform.childCount);
        }
    }

    // Play sound when card is selected
    private void OnCardSelect(Card card, bool isSelected)
    {
        if (cardClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cardClickSound);
        }
    }

    public void DebugHandState()
{
    // Output the current state of key variables
    Debug.Log("=== HAND STATE DEBUG ===");
    Debug.Log($"handSize field value: {handSize}");
    Debug.Log($"Current cards.Count: {cards.Count}");
    Debug.Log($"Transform child count: {transform.childCount}");
    
    // Check if any child slots are empty
    int emptySlots = 0;
    foreach (Transform child in transform)
    {
        if (child.childCount == 0)
        {
            emptySlots++;
            Debug.Log($"Found empty slot: {child.name}");
        }
    }
    Debug.Log($"Empty slots found: {emptySlots}");
    
    // Check if there are null entries in cards list
    int nullCards = 0;
    for (int i = 0; i < cards.Count; i++)
    {
        if (cards[i] == null)
        {
            nullCards++;
            Debug.Log($"Null card at index {i}");
        }
    }
    Debug.Log($"Null cards in list: {nullCards}");
    
    // Log info about remaining deck
    Debug.Log($"Cards left in deck: {remainingDeck.Count}");
    Debug.Log("=== END DEBUG ===");
}
}
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;

// public class HorizontalCardHolder : MonoBehaviour
// {
//     [SerializeField] private Card selectedCard;
//     [SerializeField] private Card hoveredCard;

//     [SerializeField] private GameObject slotPrefab;     // Container for each card
//     [SerializeField] private GameObject cardPrefab;     // Card prefab to instantiate
//     private RectTransform rect;

//     [Header("Spawn Settings")]
//     [SerializeField] public int handSize = 8;           // Number of cards in hand
//     public List<Card> cards;                            // Currently held cards

//     private List<GameObject> remainingDeck = new List<GameObject>(); // Cards left in the deck

//     bool isCrossing = false;                            // Prevents multiple swaps at the same time
//     [SerializeField] private bool tweenCardReturn = true; // Smooth return animation toggle

//     [Header("Audio Settings")]
//     public AudioClip cardClickSound;                    // Sound when selecting a card
//     private AudioSource audioSource;

//     // Find cards from the "Deck" GameObject that are valid and have CardInfo
//     private List<GameObject> GetSourceCardObjects()
//     {
//         List<GameObject> result = new List<GameObject>();
//         GameObject deckObj = GameObject.Find("Deck");

//         if (deckObj == null)
//         {
//             Debug.LogError("Deck object not found.");
//             return result;
//         }

//         foreach (Transform child in deckObj.transform)
//         {
//             string name = child.name.ToLower();
//             bool isValidSuit = name.Contains("spade") || name.Contains("hearts") || name.Contains("diamonds") || name.Contains("clubs");
//             bool isValidRank = name.Length > 0 && char.IsLetter(name[0]);

//             if (isValidSuit && isValidRank)
//             {
//                 if (child.GetComponent<CardInfo>() != null && child.GetComponent<CardInfo>().HasValidInfo())
//                 {
//                     result.Add(child.gameObject);
//                 }
//             }
//         }

//         return result;
//     }

//     void Start()
//     {
//         rect = GetComponent<RectTransform>();
//         cards = new List<Card>();

//         // Initialize audio source
//         audioSource = gameObject.AddComponent<AudioSource>();
//         audioSource.playOnAwake = false;

//         // Initialize and shuffle remaining deck
//         remainingDeck = GetSourceCardObjects().OrderBy(x => UnityEngine.Random.value).ToList();

//         DealInitialHand(); // Deal cards at game start
//     }

//     // Deal the initial hand at game start
//     private void DealInitialHand()
//     {
//         int cardsToDeal = Mathf.Min(handSize, remainingDeck.Count);

//         for (int i = 0; i < cardsToDeal; i++)
//         {
//             AddCardFromDeck();
//         }

//         StartCoroutine(UpdateVisualIndexes());
//     }

//     // Add a single card from the remaining deck to the hand
//     private void AddCardFromDeck()
//     {
//         if (remainingDeck.Count == 0)
//         {
//             Debug.Log("Deck is empty. No more cards to draw.");
//             return;
//         }

//         GameObject sourceCard = remainingDeck[0];
//         remainingDeck.RemoveAt(0);

//         CardInfo sourceCardInfo = sourceCard.GetComponent<CardInfo>();
//         Sprite sourceSprite = sourceCard.GetComponent<Image>()?.sprite;

//         if (sourceCardInfo == null || sourceSprite == null)
//         {
//             Debug.LogError("Invalid card in deck. Skipping.");
//             return;
//         }

//         GameObject slot = Instantiate(slotPrefab, transform);
//         GameObject cardObj = Instantiate(cardPrefab, slot.transform);

//         Card card = cardObj.GetComponent<Card>();
//         if (card == null)
//         {
//             Debug.LogError("Card prefab missing Card script.");
//             Destroy(cardObj);
//             Destroy(slot);
//             return;
//         }

//         // Initialize the new card with sprite and info
//         card.Initialize(sourceSprite, sourceCardInfo);
//         cardObj.name = $"HandCard_{sourceCardInfo.cardName}";

//         cards.Add(card);

//         // Bind interaction events
//         card.PointerEnterEvent.AddListener(CardPointerEnter);
//         card.PointerExitEvent.AddListener(CardPointerExit);
//         card.BeginDragEvent.AddListener(BeginDrag);
//         card.EndDragEvent.AddListener(EndDrag);
//         card.SelectEvent.AddListener(OnCardSelect);
//     }

//     // Fill hand back to desired size after deletion
//     public void RefillHand()
//     {
//         int cardsToAdd = handSize - cards.Count;
//         for (int i = 0; i < cardsToAdd; i++)
//         {
//             AddCardFromDeck();
//         }

//         StartCoroutine(UpdateVisualIndexes());
//     }

//     public void DeleteCard(Card cardToDelete)
//     {
//         if (cardToDelete == null)
//         {
//             Debug.LogWarning("DeleteCard called with null reference.");
//             return;
//         }

//         if (cards.Contains(cardToDelete))
//         {
//             cards.Remove(cardToDelete);
//             Destroy(cardToDelete.gameObject); // Remove from scene and memory
//             Debug.Log($"Card '{cardToDelete.name}' deleted successfully.");
//         }
//         else
//         {
//             Debug.LogWarning($"Attempted to delete a card not in hand: {cardToDelete.name}");
//         }
//     }

//     // Update visual indexes after adding/removing cards
//     private IEnumerator UpdateVisualIndexes()
//     {
//         yield return new WaitForSecondsRealtime(0.1f);
//         foreach (Card card in cards)
//         {
//             card.cardVisual?.UpdateIndex(transform.childCount);
//         }
//     }

//     private void BeginDrag(Card card)
//     {
//         selectedCard = card;
//     }

//     private void EndDrag(Card card)
//     {
//         if (selectedCard == null)
//             return;

//         // Return card to position if not dropped
//         selectedCard.transform.DOLocalMove(
//             selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero,
//             tweenCardReturn ? 0.15f : 0
//         ).SetEase(Ease.OutBack);

//         rect.sizeDelta += Vector2.right;
//         rect.sizeDelta -= Vector2.right;

//         selectedCard = null;
//     }

//     private void CardPointerEnter(Card card)
//     {
//         hoveredCard = card;
//     }

//     private void CardPointerExit(Card card)
//     {
//         hoveredCard = null;
//     }

//     void Update()
//     {
//         // Delete hovered card with Delete key
//         if (Input.GetKeyDown(KeyCode.Delete))
//         {
//             if (hoveredCard != null)
//             {
//                 Destroy(hoveredCard.transform.parent.gameObject); // Destroy slot + card
//                 cards.Remove(hoveredCard);                        // Remove from hand list
//                 RefillHand();                                     // Add new card from deck
//             }
//         }

//         // Right mouse button deselects all cards
//         if (Input.GetMouseButtonDown(1))
//         {
//             foreach (Card card in cards)
//             {
//                 card.Deselect();
//             }
//         }

//         if (selectedCard == null || isCrossing)
//             return;

//         // Handle drag-based card swapping
//         for (int i = 0; i < cards.Count; i++)
//         {
//             if (selectedCard.transform.position.x > cards[i].transform.position.x)
//             {
//                 if (selectedCard.ParentIndex() < cards[i].ParentIndex())
//                 {
//                     Swap(i);
//                     break;
//                 }
//             }

//             if (selectedCard.transform.position.x < cards[i].transform.position.x)
//             {
//                 if (selectedCard.ParentIndex() > cards[i].ParentIndex())
//                 {
//                     Swap(i);
//                     break;
//                 }
//             }
//         }
//     }

//     // Swap card with another in the hand
//     private void Swap(int index)
//     {
//         isCrossing = true;

//         Transform focusedParent = selectedCard.transform.parent;
//         Transform crossedParent = cards[index].transform.parent;

//         cards[index].transform.SetParent(focusedParent);
//         cards[index].transform.localPosition = cards[index].selected
//             ? new Vector3(0, cards[index].selectionOffset, 0)
//             : Vector3.zero;

//         selectedCard.transform.SetParent(crossedParent);
//         isCrossing = false;

//         if (cards[index].cardVisual == null) return;

//         bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
//         cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

//         // Recalculate visual positions
//         foreach (Card card in cards)
//         {
//             card.cardVisual?.UpdateIndex(transform.childCount);
//         }
//     }

//     // Play sound when card is selected
//     private void OnCardSelect(Card card, bool isSelected)
//     {
//         if (cardClickSound != null && audioSource != null)
//         {
//             audioSource.PlayOneShot(cardClickSound);
//         }
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;
    public int playerMoney;
    public Text Money;
    
    // Track which jokers the player owns, hands remaining, points, etc.
    
    // List to store the names or identifiers of owned jokers
    public List<string> ownedJokers = new List<string>();

    // Awake method to implement Singleton pattern
    private void Awake()
    {
        // Ensure only one instance of PlayerData exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps this object alive between scene changes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
        }
    }

    public void AddJoker(string jokerName)
    {
        if (!ownedJokers.Contains(jokerName))
        {
            ownedJokers.Add(jokerName);
            Debug.Log($"{jokerName} added to your collection!");
        }
    }

    public bool OwnsJoker(string jokerName)
    {
        return ownedJokers.Contains(jokerName);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

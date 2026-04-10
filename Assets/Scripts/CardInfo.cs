using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class CardInfo : MonoBehaviour
{
    // This script is attached to all card gameObject's allowing other scripts to automatically access each card's information.
    public string cardName;
    public char rankChar;
    public int rankValue;
    public int secondarySortValue; // for tie-breaking, since all face cards have value 10
    public string suitName;

    public bool HasValidInfo()
    {
        return !string.IsNullOrEmpty(cardName) && 
               rankChar != '\0' && 
               rankValue > 0 && 
               !string.IsNullOrEmpty(suitName);
    }

    // for debugging
    public override string ToString()
    {
        return $"{cardName} - {rankChar} ({rankValue}) of {suitName} with sortValue {secondarySortValue})";
    }
}

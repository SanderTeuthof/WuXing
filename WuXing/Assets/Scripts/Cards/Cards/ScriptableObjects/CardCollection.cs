using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCardCollection", menuName = "Card Collection")]
public class CardCollection : ScriptableObject
{
    public List<Card> Cards = new List<Card>();

    public void ShuffleCards()
    {
        Cards.Shuffle();
    }
}
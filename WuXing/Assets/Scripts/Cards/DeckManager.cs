using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField]
    private CardCollection _hand;
    [SerializeField]
    private CardCollection _deck;
    [SerializeField]
    private CardCollection _deckOriginal;
    [SerializeField]
    private CardCollection _used;

    private void Awake()
    {
        _hand.Cards.Clear();
        _deck.Cards.Clear();
        _used.Cards.Clear();

        _deck.Cards.AddRange(_deckOriginal.Cards);
        _deck.ShuffleCards();
    }
}

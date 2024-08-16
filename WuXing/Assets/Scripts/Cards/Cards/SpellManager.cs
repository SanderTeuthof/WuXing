using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SpellManager : MonoBehaviour
{
    [SerializeField]
    private int _maxHandSize = 5;
    [SerializeField]
    private CardCollection _hand;
    [SerializeField]
    private CardCollection _deck;
    [SerializeField]
    private CardCollection _used;
    [SerializeField]
    private IntReference _activeCard;
    [SerializeField]
    private Transform _spawnLocation;


    [HideInInspector]
    public NextSpellStrengthenEffects NextSpellStrengthenEffects = new();

    private void Start()
    {
        NextSpellStrengthenEffects.Reset();
        FillHand();
    }

    public void SelectNextCard(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        SelectCardBySteps(1);
    }

    public void SelectPreviousCard(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        SelectCardBySteps(-1);
    }

    public void SelectNextCardByTwo(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        SelectCardBySteps(2);
    }

    public void SelectPreviousCardByTwo(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        SelectCardBySteps(-2);
    }

    public void SetSelectedCardToNumber(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (ctx.control is ButtonControl buttonControl)
        {
            // Determine which key was pressed
            string keyName = buttonControl.displayName;

            // Handle number input based on the key
            if (int.TryParse(keyName, out int number))
            {
                if (number <= _hand.Cards.Count)
                    _activeCard.SetValue(number - 1);
                else
                    _activeCard.SetValue(_hand.Cards.Count - 1);
            }
        }
    }

    public void UseCard(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        _hand.Cards[_activeCard.value].UseSpell.Execute(this);
        NextSpellStrengthenEffects.Reset();
        MoveCard(_hand, _used, _hand.Cards[_activeCard.value]);
        FillHand();
    }

    public void DiscardCard(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (_hand.Cards.Count == 0)
        {
            Debug.LogWarning("Hand is empty.");
            return;
        }

        if (_hand.Cards.Count == 1)
        {
            Debug.LogWarning("You need to use your last spell!");
            return;
        }

        if (_activeCard.value >= _hand.Cards.Count)
        {
            Debug.LogWarning("Active card index is out of range.");
            return;
        }

        Card card = _hand.Cards[_activeCard.value];
        if (card == null)
        {
            Debug.LogError("Card at active index is null.");
            return;
        }

        IDiscardSpell discardSpell = card.DiscardSpell;
        if (discardSpell == null)
        {
            Debug.LogError("Discard spell is null for the selected card.");
            return;
        }

        discardSpell.Execute(this);
        MoveCard(_hand, _used, card);
        UpdateActiveIndex();
    }

    public void DrawNewCards(int cardsToDraw)
    {
        for (int i = 0; i < cardsToDraw; i++)
        {
            if (_deck.Cards.Count == 0)
                FillDeck();
            MoveCard(_deck, _hand, _deck.Cards.Last());
        }
    }

    public Transform GetSpawnLocation()
    {
        return _spawnLocation;
    }

    private void FillHand()
    {
        while (_hand.Cards.Count < _maxHandSize)
        {
            if (_deck.Cards.Count == 0)
                FillDeck();
            MoveCard(_deck, _hand, _deck.Cards.Last());
        }
    }

    private void MoveCard(CardCollection fromPile, CardCollection toPile, Card card)
    {
        if (!fromPile.Cards.Contains(card))
            return;

        fromPile.Cards.Remove(card);
        toPile.Cards.Add(card);
    }

    private void FillDeck()
    {
        _used.ShuffleCards();
        _deck.Cards.AddRange(_used.Cards);
        _used.Cards.Clear();
    }

    private void SelectCardBySteps(int steps)
    {
        int cardCount = _hand.Cards.Count;

        if (cardCount == 0)
            return;

        int newIndex = (_activeCard.value + steps) % cardCount;
        if (newIndex < 0)
            newIndex += cardCount; // Handle negative index

        _activeCard.SetValue(newIndex);
    }

    private void UpdateActiveIndex()
    {
        if (_activeCard.value >= _hand.Cards.Count)
            _activeCard.SetValue(_hand.Cards.Count - 1);
    }
}

public struct NextSpellStrengthenEffects
{
    public int AddedNumberOfSpells;
    public float DamageMultiplier;
    public float SpeedMultiplier;
    public bool SetTracking;
    public bool Penetrating;

    public void Reset()
    {
        AddedNumberOfSpells = 0;
        DamageMultiplier = 1.0f;
        SpeedMultiplier = 1.0f;
        SetTracking = false;
        Penetrating = false;
    }
}
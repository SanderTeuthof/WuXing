using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(PlayerAnimationHandler))]
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
    [SerializeField]
    private GameEvent _handChanged;


    [HideInInspector]
    public NextSpellStrengthenEffects NextSpellStrengthenEffects = new();

    private PlayerAnimationHandler _animator;

    private void Start()
    {
        _animator = GetComponent<PlayerAnimationHandler>();
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

    public void Scroll(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (ctx.action.ReadValue<Vector2>().y > 0)
            SelectCardBySteps(-1);
        else
            SelectCardBySteps(1);
        
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

        _animator.TriggerCastSpell();

        _hand.Cards[_activeCard.value].UseSpell.Execute(this);
        NextSpellStrengthenEffects.Reset();
        _handChanged.Raise(this, CardActions.Used);
        MoveCard(_hand, _used, _activeCard.value);
        FillHand();
        UpdateActiveIndex();
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
        _handChanged.Raise(this, CardActions.Discarded);
        MoveCard(_hand, _used, _activeCard.value);
        UpdateActiveIndex();
    }

    public void DrawNewCards(int cardsToDraw)
    {
        for (int i = 0; i < cardsToDraw; i++)
        {
            if (_deck.Cards.Count == 0)
                FillDeck();
            MoveCard(_deck, _hand, 0);
            _handChanged.Raise(this, CardActions.Added);
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
            MoveCard(_deck, _hand, 0);
            _handChanged.Raise(this, CardActions.Added);
        }
    }

    private void MoveCard(CardCollection fromPile, CardCollection toPile, int cardIndex)
    {
        toPile.Cards.Add(fromPile.Cards[cardIndex]);
        fromPile.Cards.RemoveAt(cardIndex);
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
            newIndex += cardCount;

        _activeCard.SetValue(newIndex);
    }

    private void UpdateActiveIndex()
    {
        while (_activeCard.value >= _hand.Cards.Count)
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

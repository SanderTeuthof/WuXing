using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardViewManager : MonoBehaviour
{
    [SerializeField]
    private float _heightFromBottom = 0; // Distance from the bottom of the screen
    [SerializeField]
    private float _spaceBetweenCards = 0.1f; // Space between each card
    [SerializeField]
    private float _cardScale = 1f; // Scale of the cards
    [SerializeField]
    private float _activeCardScaleMultiplier = 1.1f;
    [SerializeField]
    private float _flyInSpeed = 5f; // Speed at which cards fly in

    [SerializeField]
    private CardCollection _hand;
    [SerializeField]
    private IntReference _activeCard;

    private Dictionary<int, GameObject> _cardInstances = new Dictionary<int, GameObject>(); // Track card visuals

    private void Awake()
    {
        _activeCard.UseEvent();
        _activeCard.ValueChanged += SetNewCardSelected;
    }

    public void UpdateCards(Component sender, object data)
    {
        if (data is CardActions action)
        {
            switch (action)
            {
                case CardActions.Added:
                    AddNewCardVisual();
                    break;
                case CardActions.Used:
                    RemoveCardVisual();
                    break;
                case CardActions.Discarded:
                    RemoveCardVisual();
                    break;
            }

            // Reposition all cards after any change
            RepositionCards();
        }
    }

    private void AddNewCardVisual()
    {
        int cardIndex = _hand.Cards.Count - 1; // Index of the new card in the hand
        Card newCard = _hand.Cards[cardIndex]; // Get the new card

        // Instantiate the card's visual representation
        GameObject cardVisual = Instantiate(newCard.CardRepresentation, transform);

        // Set initial properties like scale and position off-screen (before flying in)
        cardVisual.transform.localScale = Vector3.one * _cardScale;
        cardVisual.transform.localPosition = CalculateCardPosition(cardIndex); // Position off-screen to the left

        cardVisual.transform.Rotate(0, 180, 0);

        _cardInstances.Add(cardIndex, cardVisual); // Add to the dictionary

    }

    private void RemoveCardVisual()
    {
        if (_cardInstances.TryGetValue(_activeCard.value, out GameObject cardVisual))
        {
            _cardInstances.Remove(_activeCard.value);
            Destroy(cardVisual);
        }

        // Adjusting indices
        Dictionary<int, GameObject> updatedInstances = new Dictionary<int, GameObject>();
        int newIdx = 0;

        foreach (var entry in _cardInstances.OrderBy(k => k.Key))
        {
            updatedInstances.Add(newIdx, entry.Value);
            newIdx++;
        }

        _cardInstances = updatedInstances;


    }

    private IEnumerator FlyInCard(GameObject card, Vector3 targetPosition)
    {
        while (Vector3.Distance(card.transform.localPosition, targetPosition) > 0.01f)
        {
            card.transform.localPosition = Vector3.MoveTowards(card.transform.localPosition, targetPosition, _flyInSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void RepositionCards()
    {
        int cardCount = _hand.Cards.Count;

        for (int i = 0; i < cardCount; i++)
        {
            if (_cardInstances.TryGetValue(i, out GameObject cardVisual))
            {
                Vector3 targetPosition = CalculateCardPosition(i);
                cardVisual.transform.localPosition = targetPosition;
            }
        }

        SetNewCardSelected(this, EventArgs.Empty);
    }

    private Vector3 CalculateCardPosition(int index)
    {
        int cardCount = _hand.Cards.Count;
        float totalWidth = (cardCount - 1) * _spaceBetweenCards; // Total width occupied by the cards
        float xPosition = (index * _spaceBetweenCards) - (totalWidth / 2); // Center cards on screen
        return new Vector3(xPosition, _heightFromBottom, 0);
    }

    private void SetNewCardSelected(object sender, EventArgs e)
    {


        foreach (var cardInstance in _cardInstances)
        {
            cardInstance.Value.transform.localScale = Vector3.one * _cardScale;

            if (cardInstance.Key == _activeCard.value)
            {
                cardInstance.Value.transform.localScale = Vector3.one * _cardScale * _activeCardScaleMultiplier;
            }
        }
    }

    private void OnDestroy()
    {
        _activeCard.OnDestroy();
    }
}
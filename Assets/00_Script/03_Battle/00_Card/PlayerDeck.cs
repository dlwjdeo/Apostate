using UnityEngine;
using System.Collections.Generic;

public class PlayerDeck : MonoBehaviour
{
    [Header("덱 설정")]
    [SerializeField] private CardDatabase cardDatabase;
    private List<CardData> deckCards = new();
    private List<CardData> handCards = new();
    private List<CardData> discardPile = new();

    /// <summary>
    /// CardDatabase로부터 덱을 초기화합니다.
    /// </summary>
    public void InitializeFromDatabase()
    {
        if (cardDatabase == null)
        {
            Debug.LogError("[PlayerDeck] CardDatabase가 설정되지 않았습니다!");
            return;
        }

        deckCards.Clear();
        handCards.Clear();
        discardPile.Clear();

        // CardDatabase의 모든 카드를 덱에 추가
        if (cardDatabase.cards != null && cardDatabase.cards.Length > 0)
        {
            deckCards.AddRange(cardDatabase.cards);
        }

        Debug.Log($"[PlayerDeck] CardDatabase로부터 덱 초기화 완료: {deckCards.Count}장");
    }

    public void InitializeDeck(List<CardData> initialCards)
    {
        deckCards.Clear();
        handCards.Clear();
        discardPile.Clear();

        // 초기 카드들을 덱에 추가
        if (initialCards != null && initialCards.Count > 0)
        {
            deckCards.AddRange(initialCards);
        }
    }

    public void Shuffle()
    {
        for (int i = deckCards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            
            CardData temp = deckCards[i];
            deckCards[i] = deckCards[randomIndex];
            deckCards[randomIndex] = temp;
        }
    }

    public bool DrawCard()
    {
        if (deckCards.Count == 0)
        {
            if (discardPile.Count == 0) return false; // 덱과 버려진 카드 모두 없는 경우

            deckCards.AddRange(discardPile);
            discardPile.Clear();
        }

        CardData drawnCard = deckCards[0];
        deckCards.RemoveAt(0);
        handCards.Add(drawnCard);

        return true;
    }

    public int DrawCards(int count)
    {
        int drawnCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (DrawCard())
                drawnCount++;
            else
                break;
        }

        return drawnCount;
    }

    public bool UseCard(CardData card)
    {
        if (handCards.Contains(card))
        {
            handCards.Remove(card);
            discardPile.Add(card);
            return true;
        }

        return false;
    }

    public bool RemoveCardFromHand(CardData card)
    {
        if (handCards.Contains(card))
        {
            handCards.Remove(card);
            deckCards.Add(card);
            return true;
        }

        return false;
    }

    public List<CardData> GetDeckCards() => new(deckCards);

    public List<CardData> GetHandCards() => new(handCards);

    public List<CardData> GetDiscardPile() => new(discardPile);

    public int GetDeckCardCount() => deckCards.Count;

    public int GetHandCardCount() => handCards.Count;

    public int GetDiscardPileCount() => discardPile.Count;
}

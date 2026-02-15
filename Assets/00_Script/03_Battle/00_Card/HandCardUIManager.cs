using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 손에 든 카드들을 UI로 표현하고 관리합니다.
/// </summary>
public class HandCardUIManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private PlayerDeck playerDeck;

    [Header("UI 설정")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardButtonUI cardButtonPrefab;
    [SerializeField] private int maxHandSize = 10;

    private List<CardButtonUI> cardButtons = new();

    private void Start()
    {
        if (playerDeck == null || battleManager == null)
        {
            Debug.LogError("[HandCardUIManager] PlayerDeck 또는 BattleManager가 설정되지 않았습니다!");
            return;
        }

        // 초기 UI 업데이트
        UpdateHandUI();
    }

    /// <summary>
    /// 손에 든 카드 UI를 업데이트합니다.
    /// Void는 드로우나 카드 사용 후 호출됩니다.
    /// </summary>
    public void UpdateHandUI()
    {
        var handCards = playerDeck.GetHandCards();
        Debug.Log($"[HandCardUIManager] UI 업데이트 시작: {handCards.Count}장");

        // 기존 버튼 정리
        foreach (var btn in cardButtons)
        {
            Destroy(btn.gameObject);
        }
        cardButtons.Clear();

        // 카드 버튼 프리팹이 없으면 경고
        if (cardButtonPrefab == null)
        {
            Debug.LogError("[HandCardUIManager] CardButtonUI 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 카드 컨테이너가 없으면 경고
        if (cardContainer == null)
        {
            Debug.LogError("[HandCardUIManager] cardContainer가 설정되지 않았습니다!");
            return;
        }

        // 새로운 버튼 생성
        for (int i = 0; i < handCards.Count && i < maxHandSize; i++)
        {
            CardData cardData = handCards[i];
            CardButtonUI btn = Instantiate(cardButtonPrefab, cardContainer);
            btn.Initialize(cardData, this);
            cardButtons.Add(btn);
            Debug.Log($"[HandCardUIManager] 버튼 생성: {cardData.cardName}");
        }

        Debug.Log($"[HandCardUIManager] 손 카드 UI 업데이트 완료: {handCards.Count}장");
    }

    /// <summary>
    /// 카드 버튼이 클릭되었을 때 호출됩니다.
    /// </summary>
    public void OnCardButtonClicked(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[HandCardUIManager] 카드 데이터가 없습니다!");
            return;
        }

        Debug.Log($"[HandCardUIManager] {cardData.cardName} 클릭 (비용: {cardData.cost})");

        // 카드 사용 시도
        var enemies = battleManager.EnemyUnits;
        if (enemies.Count > 0)
        {
            Unit target = enemies[0] as Unit;
            battleManager.TryUseCard(cardData, new[] { target });
            // UI 업데이트는 BattleManager에서 처리됨
        }
        else
        {
            Debug.LogWarning("[HandCardUIManager] 타겟이 없습니다!");
        }
    }
}

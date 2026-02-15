using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Unit playerUnit;
    [SerializeField] private List<Unit> enemyUnits = new List<Unit>();
    [SerializeField] private CardExecutor cardExecutor;
    [SerializeField] private PlayerDeck playerDeck;
    [SerializeField] private HandCardUIManager handCardUIManager;

    [Header("턴 설정")]
    [SerializeField] private int initialCostPerTurn = 3;
    [SerializeField] private float turnTransitionDelay = 0.5f;

    private BattleState currentState = BattleState.Idle;
    private int turnCount = 0;
    private int playerTurnCount = 0;
    private int enemyTurnCount = 0;

    public event Action<BattleState> OnStateChanged;
    public event Action<Unit> OnTurnStart;
    public event Action<Unit> OnTurnEnd;
    public event Action<BattleResult> OnBattleEnd;
    public BattleState CurrentState => currentState;
    public int TurnCount => turnCount;
    public Unit PlayerUnit => playerUnit;
    public IReadOnlyList<Unit> EnemyUnits => enemyUnits;
    public Unit CurrentTurnUnit
    {
        get
        {
            if (currentState == BattleState.PlayerTurn) return playerUnit;
            if (currentState == BattleState.EnemyTurn)
            {
                foreach (var u in enemyUnits)
                    if (u != null && !u.IsDead) return u;
            }
            return null;
        }
    }

    private void Start()
    {
        if (playerUnit == null || enemyUnits == null || enemyUnits.Count == 0)
        {
            return;
        }

        InitializeBattle();
    }

    public void InitializeBattle()
    {
        turnCount = 0;
        playerTurnCount = 0;
        enemyTurnCount = 0;

        // PlayerDeck 초기화
        if (playerDeck != null)
        {
            // CardDatabase로부터 덱 초기화
            playerDeck.InitializeFromDatabase();
        }
        else
        {
            Debug.LogError("[BattleManager] PlayerDeck이 설정되지 않았습니다!");
            return;
        }

        // CardExecutor에 PlayerDeck 설정
        if (cardExecutor != null && playerDeck != null)
        {
            cardExecutor.SetPlayerDeck(playerDeck);
            // 배틀 시작 시 덱 셔플
            playerDeck.Shuffle();
        }

        // 초기 비용 설정
        playerUnit.ResetCost();
        playerUnit.RestoreCost(initialCostPerTurn);

        foreach (var e in enemyUnits)
        {
            if (e == null) continue;
            e.ResetCost();
            e.RestoreCost(initialCostPerTurn);
        }

        SetState(BattleState.PlayerTurn);
    }

    private void SetState(BattleState newState)
    {
        if (currentState == newState) return;

        BattleState previousState = currentState;
        currentState = newState;

        OnStateChanged?.Invoke(newState);

        switch (newState)
        {
            case BattleState.PlayerTurn:
                StartPlayerTurn();
                break;

            case BattleState.EnemyTurn:
                StartEnemyTurn();
                break;

            case BattleState.TurnEnd:
                StartTurnEnd();
                break;

            case BattleState.PlayerWin:
            case BattleState.PlayerLose:
                EndBattle();
                break;
        }
    }
    private void StartPlayerTurn()
    {
        playerTurnCount++;
        turnCount++;

        playerUnit.ResetCost();
        playerUnit.RestoreCost(initialCostPerTurn);

        // 카드 드로우 (기본값 3장)
        if (playerDeck != null)
        {
            playerDeck.DrawCards(initialCostPerTurn);

            // UI 업데이트
            if (handCardUIManager != null)
            {
                handCardUIManager.UpdateHandUI();
            }
        }

        OnTurnStart?.Invoke(playerUnit);
    }

    public void EndPlayerTurn()
    {
        if (currentState != BattleState.PlayerTurn) return;

        OnTurnEnd?.Invoke(playerUnit);
        StartCoroutine(TransitionToNextTurn());
    }

    private void StartEnemyTurn()
    {
        enemyTurnCount++;
        turnCount++;

        foreach (var e in enemyUnits)
        {
            if (e == null) continue;
            e.ResetCost();
            e.RestoreCost(initialCostPerTurn);
        }

        StartCoroutine(ProcessAllEnemiesTurn());
    }

    private IEnumerator ProcessAllEnemiesTurn()
    {
        var units = enemyUnits.ToArray();
        foreach (var unit in units)
        {
            if (unit == null || unit.IsDead) continue;

            OnTurnStart?.Invoke(unit);

            // TODO: 적 AI 로직 호출 또는 Unit에 행동 위임
            yield return StartCoroutine(ProcessEnemyTurn(unit));

            OnTurnEnd?.Invoke(unit);
            yield return new WaitForSeconds(0.3f);
        }

        StartCoroutine(TransitionToNextTurn());
    }

    private IEnumerator ProcessEnemyTurn(Unit unit)
    {
        // 기본 대기(애니메이션/이펙트 등)
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator TransitionToNextTurn()
    {
        yield return new WaitForSeconds(turnTransitionDelay);

        // 전투 종료 체크
        if (CheckBattleEnd())
            yield break;

        // 턴 종료 처리 (버프/디버프 감소 등)
        SetState(BattleState.TurnEnd);
        yield return new WaitForSeconds(0.3f);

        // 다음 턴 시작
        if (currentState == BattleState.PlayerTurn)
            SetState(BattleState.EnemyTurn);
        else
            SetState(BattleState.PlayerTurn);
    }

    private void StartTurnEnd()
    {
        // TODO: 버프/디버프 감소 로직 추가
    }
    private bool CheckBattleEnd()
    {
        if (playerUnit.IsDead)
        {
            SetState(BattleState.PlayerLose);
            return true;
        }
        // 전투 종료: 모든 적이 죽었는지 확인
        bool anyAlive = false;
        foreach (var e in enemyUnits)
        {
            if (e != null && !e.IsDead) { anyAlive = true; break; }
        }
        if (!anyAlive)
        {
            SetState(BattleState.PlayerWin);
            return true;
        }

        return false;
    }

    private void EndBattle()
    {
        BattleResult result = new BattleResult
        {
            IsPlayerWin = currentState == BattleState.PlayerWin,
            TotalTurns = turnCount,
            PlayerTurns = playerTurnCount,
            EnemyTurns = enemyTurnCount,
            FinalPlayerHp = playerUnit.CurrentHp,
            FinalEnemyHp = GetTotalEnemyHp()
        };

        OnBattleEnd?.Invoke(result);
        Debug.Log($"[BattleManager] 전투 종료: {(result.IsPlayerWin ? "플레이어 승리" : "플레이어 패배")}");
    }

    public bool TryUseCard(CardData card, Unit target)
    {
        return TryUseCard(card, new[] { target });
    }

    public bool TryUseCard(CardData card, Unit[] targets)
    {
        if (card == null)
        {
            Debug.LogWarning("[BattleManager] 카드가 없습니다!");
            return false;
        }

        if (currentState != BattleState.PlayerTurn)
        {
            Debug.LogWarning("[BattleManager] 플레이어 턴이 아닙니다!");
            return false;
        }

        if (playerUnit.CurrentCost < card.cost)
        {
            Debug.LogWarning("[BattleManager] 비용이 부족합니다!");
            return false;
        }

        // 비용 소비
        playerUnit.ConsumeCost(card.cost);

        // 카드 효과 실행
        if (cardExecutor != null)
        {
            var result = cardExecutor.Execute(card, playerUnit, targets);
            Debug.Log($"[BattleManager] {result}");
        }
        else
        {
            Debug.LogWarning("[BattleManager] CardExecutor가 설정되지 않았습니다!");
            return false;
        }

        // 손에서 카드 제거
        if (playerDeck != null)
        {
            playerDeck.UseCard(card);

            // UI 업데이트
            if (handCardUIManager != null)
            {
                handCardUIManager.UpdateHandUI();
            }
        }
        else
        {
            Debug.LogWarning("[BattleManager] PlayerDeck이 설정되지 않았습니다!");
        }

        return true;
    }

    private bool TryUseCardOld(int costRequired)
    {
        if (currentState != BattleState.PlayerTurn)
        {
            Debug.LogWarning("[BattleManager] 플레이어 턴이 아닙니다!");
            return false;
        }

        if (playerUnit.CurrentCost < costRequired)
        {
            Debug.LogWarning("[BattleManager] 비용이 부족합니다!");
            return false;
        }

        playerUnit.ConsumeCost(costRequired);
        return true;
    }


    [ContextMenu("즉시 전투 종료 - 플레이어 승리")]
    private void DebugWin()
    {
        foreach (var e in enemyUnits)
        {
            if (e == null) continue;
            e.GetDamage(999);
        }
        CheckBattleEnd();
    }

    [ContextMenu("즉시 전투 종료 - 플레이어 패배")]
    private void DebugLose()
    {
        playerUnit.GetDamage(999);
        CheckBattleEnd();
    }

    private int GetTotalEnemyHp()
    {
        int sum = 0;
        foreach (var e in enemyUnits)
            if (e != null) sum += e.CurrentHp;
        return sum;
    }
}

public class BattleResult
{
    public bool IsPlayerWin;
    public int TotalTurns;
    public int PlayerTurns;
    public int EnemyTurns;
    public int FinalPlayerHp;
    public int FinalEnemyHp;
}

using System;
using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Unit playerUnit;
    [SerializeField] private Unit enemyUnit;
    [SerializeField] private CardExecutor cardExecutor;

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
    public Unit EnemyUnit => enemyUnit;
    public Unit CurrentTurnUnit => 
        currentState == BattleState.PlayerTurn ? playerUnit :
        currentState == BattleState.EnemyTurn ? enemyUnit : null;

    private void Start()
    {
        if (playerUnit == null || enemyUnit == null)
        {
            Debug.LogError("[BattleManager] 플레이어 또는 적 유닛이 설정되지 않았습니다!");
            return;
        }

        InitializeBattle();
    }

    public void InitializeBattle()
    {
        turnCount = 0;
        playerTurnCount = 0;
        enemyTurnCount = 0;

        // 초기 비용 설정
        playerUnit.ResetCost();
        playerUnit.RestoreCost(initialCostPerTurn);
        
        enemyUnit.ResetCost();
        enemyUnit.RestoreCost(initialCostPerTurn);

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

        enemyUnit.ResetCost();
        enemyUnit.RestoreCost(initialCostPerTurn);

        OnTurnStart?.Invoke(enemyUnit);
        
        StartCoroutine(ProcessEnemyTurn());
    }

    private IEnumerator ProcessEnemyTurn()
    {
        yield return new WaitForSeconds(1.5f);
        
        // TODO: 적 AI 로직 호출

        OnTurnEnd?.Invoke(enemyUnit);
        StartCoroutine(TransitionToNextTurn());
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

        if (enemyUnit.IsDead)
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
            FinalEnemyHp = enemyUnit.CurrentHp
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
        enemyUnit.GetDamage(999);
        CheckBattleEnd();
    }

    [ContextMenu("즉시 전투 종료 - 플레이어 패배")]
    private void DebugLose()
    {
        playerUnit.GetDamage(999);
        CheckBattleEnd();
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

using UnityEngine;
using System;
using System.Collections.Generic;

public class CardExecutor : MonoBehaviour
{
    [Header("드로우 설정")]
    [SerializeField] private int defaultDrawCount = 1;
    private PlayerDeck playerDeck;

    // 카드 사용 결과 이벤트
    public event Action<CardExecutionResult> OnCardExecuted;

    public void SetPlayerDeck(PlayerDeck deck)
    {
        playerDeck = deck;
    }

    public CardExecutionResult Execute(CardData card, Unit caster, params Unit[] targets)
    {
        if (card == null)
        {
            Debug.LogError("[CardExecutor] 카드가 없습니다!");
            return null;
        }

        if (caster == null)
        {
            Debug.LogError("[CardExecutor] 캐스터가 없습니다!");
            return null;
        }

        var result = new CardExecutionResult { cardName = card.cardName };

        // 각 효과 처리
        foreach (var effect in card.cardEffects)
        {
            if (effect == null) continue;

            // 타겟 검증
            var validTargets = ValidateTargets(effect, caster, targets);
            if (validTargets.Length == 0)
            {
                Debug.LogWarning($"[CardExecutor] {card.cardName}의 {effect.cardType} 효과에 유효한 타겟이 없습니다!");
                continue;
            }

            // 효과 적용
            ApplyEffect(effect, caster, validTargets, result);
        }

        OnCardExecuted?.Invoke(result);
        return result;
    }

    private Unit[] ValidateTargets(CardEffectBase effect, Unit caster, Unit[] targets)
    {
        if (!(effect is AttackEffect attackEffect))
            return targets.Length > 0 ? targets : new Unit[0];

        var targetType = attackEffect.targetType;

        // Self 타입인 경우
        if (targetType == TargetType.Self)
            return new[] { caster };

        // Enemy/AllEnemies 타입인 경우
        return targets.Length > 0 ? targets : new Unit[0];
    }

    private void ApplyEffect(CardEffectBase effect, Unit caster, Unit[] targets, CardExecutionResult result)
    {
        switch (effect)
        {
            case AttackEffect attackEffect:
                ApplyAttack(attackEffect, caster, targets, result);
                break;

            case HealEffect healEffect:
                ApplyHeal(healEffect, targets, result);
                break;

            case BarrierEffect barrierEffect:
                ApplyBarrier(barrierEffect, targets, result);
                break;

            case DebuffEffect debuffEffect:
                ApplyDebuff(debuffEffect, targets, result);
                break;

            case DrawEffect drawEffect:
                ApplyDraw(drawEffect, result);
                break;

            case BonusDamageIfDebuffEffect bonusEffect:
                ApplyBonusDamage(bonusEffect, caster, targets, result);
                break;

            default:
                Debug.LogWarning($"[CardExecutor] 처리되지 않은 효과 타입: {effect.GetType().Name}");
                break;
        }
    }

    private void ApplyAttack(AttackEffect effect, Unit caster, Unit[] targets, CardExecutionResult result)
    {
        foreach (var target in targets)
        {
            if (target == null) continue;

            // 기본 데미지 + 백분율 데미지
            int baseDamage = effect.damage;
            int percentageDamage = Mathf.RoundToInt(caster.MaxHp * effect.perentageDamage / 100f);
            int totalDamage = baseDamage + percentageDamage;

            target.GetDamage(totalDamage);
            result.damageDealt += totalDamage;

            Debug.Log($"[CardExecutor] {target.name}에게 {totalDamage} 데미지!");
        }
    }

    private void ApplyHeal(HealEffect effect, Unit[] targets, CardExecutionResult result)
    {
        foreach (var target in targets)
        {
            if (target == null) continue;

            int oldHp = target.CurrentHp;
            target.Heal(effect.heal);
            int actualHealed = target.CurrentHp - oldHp;
            result.healAmount += actualHealed;

            Debug.Log($"[CardExecutor] {target.name}이 {actualHealed} 회복!");
        }
    }

    private void ApplyBarrier(BarrierEffect effect, Unit[] targets, CardExecutionResult result)
    {
        foreach (var target in targets)
        {
            if (target == null) continue;

            target.AddBarrier(effect.barrierAmount);
            result.barrierGained += effect.barrierAmount;

            Debug.Log($"[CardExecutor] {target.name}이 {effect.barrierAmount} 배리어 획득!");
        }
    }

    private void ApplyDebuff(DebuffEffect effect, Unit[] targets, CardExecutionResult result)
    {
        foreach (var target in targets)
        {
            if (target == null) continue;

            // 임시: 로그만 출력 (BuffManager 구현 후 통합)
            Debug.Log($"[CardExecutor] {target.name}에게 {effect.debuffType} 디버프 {effect.durationTurns}턴 적용!");
            result.debuffsApplied++;
        }
    }

    private void ApplyDraw(DrawEffect effect, CardExecutionResult result)
    {
        int drawCount = effect.drawCount > 0 ? effect.drawCount : defaultDrawCount;

        if (playerDeck != null)
        {
            int actualDrawn = playerDeck.DrawCards(drawCount);
            Debug.Log($"[CardExecutor] {actualDrawn}장의 카드를 드로우했습니다!");
            result.cardsDraw = actualDrawn;
        }
        else
        {
            Debug.LogWarning("[CardExecutor] PlayerDeck이 설정되지 않았습니다!");
        }
    }

    private void ApplyBonusDamage(BonusDamageIfDebuffEffect effect, Unit caster, Unit[] targets, CardExecutionResult result)
    {
        foreach (var target in targets)
        {
            if (target == null) continue;

            // 임시: 항상 보너스 데미지 적용 (BuffManager 구현 후 조건 추가)
            int bonusDamage = effect.bonusDamageFlat + effect.bonusDamagePerStack;
            target.GetDamage(bonusDamage);
            result.damageDealt += bonusDamage;

            Debug.Log($"[CardExecutor] {target.name}에게 추가 {bonusDamage} 데미지!");
        }
    }
}

public class CardExecutionResult
{
    public string cardName;
    public int damageDealt = 0;
    public int healAmount = 0;
    public int barrierGained = 0;
    public int debuffsApplied = 0;
    public int cardsDraw = 0;

    public override string ToString()
    {
        return $"[{cardName}] 데미지: {damageDealt}, 치유: {healAmount}, 배리어: {barrierGained}";
    }
}
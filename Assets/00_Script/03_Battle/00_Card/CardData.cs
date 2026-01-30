using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;
    [TextArea]public string description;
    public int cost;
    public Sprite cardImage;

    [SerializeReference] 
    public List<CardEffectBase> cardEffects = new();

    [ContextMenu("Attack")]
    private void AddAttack() => cardEffects.Add(new AttackEffect());
    [ContextMenu("Heal")]
    private void AddHeal() => cardEffects.Add(new HealEffect());
    [ContextMenu("Draw")]
    private void AddDraw() => cardEffects.Add(new DrawEffect());
    [ContextMenu("Barrier")]
    private void AddBarrier() => cardEffects.Add(new BarrierEffect());
    [ContextMenu("Debuff")]
    private void AddDebuff() => cardEffects.Add(new DebuffEffect());

}

public enum CardType
{
    Attack,
    Barrier,
    Draw,
    Heal,
    Debuff,
}
public enum TargetType
{
    Self,
    Enemy,
    AllEnemies,
}
[Serializable]
public abstract class CardEffectBase
{
    public abstract CardType cardType { get; }
}

[Serializable]
public class AttackEffect : CardEffectBase
{
    public override CardType cardType => CardType.Attack;
    public TargetType targetType;

    public int damage;
    public float perentageDamage;
}
[Serializable]
public class HealEffect : CardEffectBase
{
    public override CardType cardType => CardType.Heal;
    public TargetType targetType;
    public int heal;
}
[Serializable]
public class DrawEffect : CardEffectBase
{
    public override CardType cardType => CardType.Draw;
    public TargetType targetType;
    public int drawCount;
}
[Serializable]
public class BarrierEffect : CardEffectBase
{
    public override CardType cardType => CardType.Barrier;
    public TargetType targetType;
    public int barrierAmount;
}

public enum DebuffType
{
    Weak,
    Stun,
    Bleed,
}

[Serializable]
public class DebuffEffect : CardEffectBase
{
    public override CardType cardType => CardType.Debuff;
    public TargetType targetType;
    public DebuffType debuffType;
    public float debuff;
    public int debuffAmount;
    public int durationTurns;
}

[Serializable]
public class BonusDamageIfDebuffEffect : CardEffectBase
{
    public override CardType cardType => CardType.Attack;

    public TargetType targetType = TargetType.Enemy;

    public DebuffType requiredDebuff = DebuffType.Bleed;

    public int bonusDamageFlat = 0;     // √‚«˜¿Ã∏È +N
    public int bonusDamagePerStack = 0; // √‚«˜ Ω∫≈√¥Á +N
}
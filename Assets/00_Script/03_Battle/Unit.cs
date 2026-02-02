using UnityEngine;
using System;

public abstract class Unit : MonoBehaviour
{
    [SerializeField] private UnitType unitType;
    [SerializeField] private int maxHp;
    [SerializeField] private int currentHp;
    [SerializeField] private int barrier;
    [SerializeField] private int maxCost;
    [SerializeField] private int currentCost;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;
    public int Barrier => barrier;
    public int MaxCost => maxCost;
    public int CurrentCost => currentCost;
    public bool IsDead => currentHp <= 0;
    public UnitType Type => unitType;

    public event Action OnDead;
    public event Action<int> OnDamageReceived;
    public event Action<int> OnHealed;


    public void GetDamage(int damage)
    {
        int remainingDamage = damage;
        
        if (barrier > 0)
        {
            if (barrier >= remainingDamage)
            {
                barrier -= remainingDamage;
                remainingDamage = 0;
            }
            else
            {
                remainingDamage -= barrier;
                barrier = 0;
            }
        }
        
        if (remainingDamage > 0)
        {
            currentHp -= remainingDamage;
            
            if (currentHp < 0)
                currentHp = 0;
            
            OnDamageReceived?.Invoke(damage);
            
            if (currentHp <= 0)
                Dead();
        }
    }

    public void Heal(int healAmount)
    {
        if(healAmount <= 0) return;
        
        int oldHp = currentHp;
        currentHp = Mathf.Min(currentHp + healAmount, maxHp);
        int actualHealed = currentHp - oldHp;
        
        OnHealed?.Invoke(actualHealed);
    }
    public void AddBarrier(int barrierAmount)
    {
        barrier += barrierAmount;
    }
    public void ResetBarrier()
    {
        barrier = 0;
    }

    public bool ConsumeCost(int amount)
    {
        if (currentCost < amount)
            return false;

        currentCost -= amount;
        return true;
    }

    public void RestoreCost(int amount)
    {
        currentCost = Mathf.Min(currentCost + amount, maxCost);
    }

    public void ResetCost()
    {
        currentCost = 0;
    }

    public void Dead()
    {
        currentHp = 0;
        OnDead?.Invoke();
    }
}

public enum UnitType
{
    Player,
    Enemy,
}
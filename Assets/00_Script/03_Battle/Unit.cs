using NUnit.Framework;
using UnityEngine;

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
            {
                Dead();
            }
        }
    }

    public void Heal(int healAmount)
    {
        if(healAmount <= 0) return;
        currentHp = Mathf.Min(currentHp + healAmount, maxHp);
    }
    public void AddBarrier(int barrierAmount)
    {
        barrier += barrierAmount;
    }
    public void ResetBarrier()
    {
        barrier = 0;
    }

    public void Dead()
    {
        currentHp = 0;
        // Additional logic for when the unit dies can be added here
    }
}

public enum UnitType
{
    Player,
    Enemy,
}